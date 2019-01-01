using Assets.Scripts.TSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TS;

namespace TS2
{
    public class Map
    {
        public MatInfo[] Materials;
        public List<Section> Sections        = new List<Section>();
        public List<VisPortal> VisPortals    = new List<VisPortal>();
        public List<PortalDoor> PortalDoors  = new List<PortalDoor>();

        public List<Tuple<int, float[]>> PossablePositions = new List<Tuple<int, float[]>>();
        public List<float[]> Positions                     = new List<float[]>();
        public List<float[]> Boxes                         = new List<float[]>();

        public Map() { }

        public Map(byte[] Data)
        {
            Load(Data);
        }

        public void Load(byte[] Data)
        {
            using (BinaryReader r = new BinaryReader(new MemoryStream(Data)))
            {
                uint materialInfoOffset = r.ReadUInt32();
                uint indexOffset        = r.ReadUInt32();
                uint dataStart          = r.ReadUInt32();
                uint entityDefOffset    = r.ReadUInt32();

                Materials = MatInfo.ReadMatInfos(r, materialInfoOffset).ToArray();

                r.BaseStream.Seek(indexOffset, SeekOrigin.Begin);
                uint numSections = (dataStart - indexOffset) / SectionInfo.SIZE;
                var sectionInfos = ReadSectionInfos(r, numSections);

                for (int i = 0; i < numSections; i++)
                {
                    var sectionInfo = sectionInfos[i];

                    if (sectionInfo.DataOffset != 0)
                    {
                        LoadSection(r, sectionInfo, i);
                    }
                }

                // Feels a bit hacky
                var visPortalsOffset = sectionInfos[sectionInfos.Length - 2].DataOffset;
                var visPortalsLength = sectionInfos[2].LinksOffset - visPortalsOffset;
                LoadVisPortals(r, visPortalsOffset, visPortalsLength);

                PortalDoors = LoadPortalDoors(r, entityDefOffset);

                // Scan the rest of the file for positions

                PossablePositions = Utils.ScanForVector3(r, -200.0f, 200.0f, 2112332);

                // Some section related points
                /*
                var offsets = new List<uint>();
                foreach (var section in sectionInfos)
                {
                    r.BaseStream.Seek(section.StuffOffset, SeekOrigin.Begin);
                    for (int i = 0; i < 10000; i++)
                    {
                        var offset = r.ReadUInt32();
                        if (offset != 0)
                        {
                            offsets.Add(offset);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                for (int i = 0; i < offsets.Count; i++)
                {
                    var offset = offsets[i];
                    r.BaseStream.Seek(offset, SeekOrigin.Begin);

                    //for (int aye = 0; aye < 4; aye++)
                    {
                        var posAndRotation = new float[6];
                        for (int eye = 0; eye < posAndRotation.Length; eye++)
                        {
                            posAndRotation[eye] = r.ReadSingle();
                        }

                        Positions.Add(posAndRotation);
                    }
                }*/

                // Testing
                /*var boxesOffset = sectionInfos[sectionInfos.Length - 2].DataOffset;
                r.BaseStream.Seek(boxesOffset + 48, SeekOrigin.Begin);

                for (int i = 0; i < 50; i++)
                {
                    r.BaseStream.Seek(24, SeekOrigin.Current);

                    var points = new List<float>();
                    for (int y = 0; y < 12; y++)
                    {
                        var point = r.ReadSingle();
                        points.Add(point);
                    }

                    Boxes.Add(points.ToArray());

                    //r.BaseStream.Seek(8, SeekOrigin.Current);
                }*/
            }
        }

        private void LoadSection(BinaryReader R, SectionInfo SectionInfo, int Index)
        {
            uint offset = SectionInfo.DataOffset - MeshInfo.SIZE;

            R.BaseStream.Seek(offset, SeekOrigin.Begin);
            var meshInfo = MeshInfo.Read(R);
            var mesh     = SubMesh.Load(R, meshInfo, false);

            var section = new Section()
            {
                ID   = Index,
                Mesh = mesh
            };

            Sections.Add(section);
        }

        public SectionInfo[] ReadSectionInfos(BinaryReader R, uint NumSections)
        {
            var sections = new SectionInfo[NumSections];

            for (int i = 0; i < NumSections; i++)
            {
                sections[i] = ReadSectionInfo(R);
            }

            return sections;
        }

        public SectionInfo ReadSectionInfo(BinaryReader R)
        {
            var info = new SectionInfo()
            {
                DataOffset  = R.ReadUInt32(),
                LinksOffset = R.ReadUInt32(),
                StuffOffset = R.ReadUInt32()
            };

            R.BaseStream.Seek(172 - 8, SeekOrigin.Current);

            return info;
        }

        private void LoadVisPortals(BinaryReader R, uint Offset, uint Size)
        {
            const int HEADER_SIZE = 48;
            R.BaseStream.Seek(Offset, SeekOrigin.Begin);
            R.BaseStream.Seek(HEADER_SIZE, SeekOrigin.Current);

            var numPortals = ((Size - HEADER_SIZE) / VisPortal.SIZE);
            VisPortals     = new List<VisPortal>((int)numPortals);

            var lastPos = R.BaseStream.Position;

            for (int i = 0; i < numPortals; i++)
            {
                var visPortal = VisPortal.Read(R);
                VisPortals.Add(visPortal);

                var bytesRead = R.BaseStream.Position - lastPos;
                lastPos       = R.BaseStream.Position;
                Debug.WriteLine($"Offset: {R.BaseStream.Position}, read {bytesRead} bytes");
            }
        }

        private List<PortalDoor> LoadPortalDoors(BinaryReader R, uint Offset)
        {
            if (Offset > 0)
            {
                R.BaseStream.Seek(Offset, SeekOrigin.Begin);
                var numDoors    = R.ReadInt32();
                var portalDoors = new List<PortalDoor>(numDoors);

                for (int i = 0; i < numDoors; i++)
                {
                    var portalDoor = PortalDoor.Read(R);
                    portalDoors.Add(portalDoor);
                }

                return portalDoors;
            }
            else
            {
                return null;
            }
        }
    }

    [System.Serializable]
    public struct SectionInfo
    {
        public const int SIZE = 176;

        public uint DataOffset;
        public uint LinksOffset;
        public uint StuffOffset;
    }

    public struct Section
    {
        public int ID;
        public SubMesh Mesh;
    }

    [System.Serializable]
    public struct VisPortal
    {
        public const int SIZE = 72;

        public uint ID;
        public uint UNK;
        public float[][] Points; // 12 floats, 4 sets of 3, verts of the portal plane

        public static VisPortal Read(BinaryReader R)
        {
            const int NUM_POINTS       = 4; // verts of the portal
            const int NUM_VEC_ELEMENTS = 3; // x,y,z pos of the plane vert

            var visPortal  = new VisPortal();
            visPortal.ID   = R.ReadUInt32();
            visPortal.UNK  = R.ReadUInt32();

            R.BaseStream.Seek(16, SeekOrigin.Current);
            visPortal.Points = new float[NUM_POINTS][];
            for (int i = 0; i < NUM_POINTS; i++)
            {
                visPortal.Points[i] = new float[NUM_VEC_ELEMENTS];
                var bytes           = R.ReadBytes(NUM_VEC_ELEMENTS * 4);
                Buffer.BlockCopy(bytes, 0, visPortal.Points[i], 0, bytes.Length);
            }

            return visPortal;
        }
    }

    [System.Serializable]
    public struct PortalDoor
    {
        public const int SIZE = 44;

        public uint ID1;
        public uint ID2;
        public float[] Position;
        public float[] Dimensions;
        public uint UNK;
        public float Angle;
        public uint UNK3;

        public static PortalDoor Read(BinaryReader R)
        {
            var entityDef = new PortalDoor()
            {
                ID1         = R.ReadUInt32(),
                ID2         = R.ReadUInt32(),
                Position    = R.ReadSingleArray(3),
                Dimensions  = R.ReadSingleArray(3),
                UNK         = R.ReadUInt32(),
                Angle       = R.ReadSingle(),
                UNK3        = R.ReadUInt32()
            };

            return entityDef;
        }
    }
}
