using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace TS2
{
    public class Map
    {
        public MatInfo[] Materials;
        public List<Section> Sections = new List<Section>();

        public List<TS2.Vertex> VertsTemp = new List<Vertex>();
        public List<TS2.Vertex> NormsTemp = new List<Vertex>();

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
            }
        }

        /*private void LoadSection(BinaryReader R, SectionInfo SectionInfo)
        {
            uint offset = SectionInfo.DataOffset - MeshInfo.SIZE;

            R.BaseStream.Seek(offset, SeekOrigin.Begin);
            var meshInfo = MeshInfo.Read(R);

            R.BaseStream.Seek(meshInfo.VertsOffset, SeekOrigin.Begin);
            for (int i = 0; i < meshInfo.NumVerts; i++)
            {
                var vert = Vertex.Read(R);
                VertsTemp.Add(vert);
            }

            R.BaseStream.Seek(meshInfo.NormalsOffset, SeekOrigin.Begin);
            for (int i = 0; i < meshInfo.NumVerts; i++)
            {
                var vert = Vertex.Read(R);
                NormsTemp.Add(vert);
            }
        }*/

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
    }

    public struct SectionInfo
    {
        public const int SIZE = 176;

        public uint DataOffset;
    }

    public struct Section
    {
        public Mesh Mesh;
    }
}
