using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace TS2
{
    public class Map
    {
        public MatInfo[] Materials;
        public List<Section> Sections      = new List<Section>();
        public List<VisPortal> VisPortals  = new List<VisPortal>();

        public List<float[]> Boxes = new List<float[]>();

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

                Materials = MatInfo.ReadMatInfos(r, materialInfoOffset).ToArray();

                r.BaseStream.Seek(indexOffset, SeekOrigin.Begin);
                uint numSections = (dataStart - indexOffset) / SectionInfo.SIZE;
                var sectionInfos = ReadSectionInfos(r, numSections);

                for (int i = 0; i < numSections; i++)
                {
                    var sectionInfo = sectionInfos[i];

                    if (sectionInfo.DataOffset != 0)
                    {
                        LoadSection(r, sectionInfo);
                    }
                }

                var boxesOffset = sectionInfos[sectionInfos.Length - 2].DataOffset + 48;
                LoadVisPortals(r, boxesOffset, 50);

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

        private void LoadSection(BinaryReader R, SectionInfo SectionInfo)
        {
            uint offset = SectionInfo.DataOffset - MeshInfo.SIZE;

            R.BaseStream.Seek(offset, SeekOrigin.Begin);
            var meshInfo = MeshInfo.Read(R);

            var mesh = Mesh.Load(R, meshInfo);

            var section = new Section()
            {
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
                DataOffset = R.ReadUInt32()
            };

            R.BaseStream.Seek(172, SeekOrigin.Current);

            return info;
        }

        private void LoadVisPortals(BinaryReader R, uint Offset, int NumVisPortals)
        {
            R.BaseStream.Seek(Offset, SeekOrigin.Begin);
            VisPortals = new List<VisPortal>(NumVisPortals);

            for (int i = 0; i < NumVisPortals; i++)
            {
                var visPortal = VisPortal.Read(R);
                VisPortals.Add(visPortal);
            }
        }
    }

    public struct SectionInfo
    {
        public const int SIZE = 176;

        public uint DataOffset;
        public uint LinksOffset;
    }

    public struct Section
    {
        public Mesh Mesh;
    }

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
}
