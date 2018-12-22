using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS2
{
    public struct MeshInfo
    {
        public static uint SIZE = 144;

        public byte ID;
        public MeshInfoOffsets MeshOffsets;
        public MeshInfoOffsets MeshOffsets2;
        public MeshInfoOffsets TransparentMeshOffsets;

        public static MeshInfo Read(BinaryReader R)
        {
            var meshInfo = new MeshInfo();

            R.BaseStream.Seek(3, SeekOrigin.Current);
            meshInfo.ID = R.ReadByte();

            R.BaseStream.Seek(16, SeekOrigin.Current);
            var MatIdOffset      = R.ReadUInt32();
            var MatIdOffset2     = R.ReadUInt32();
            var MatIdOffset3     = R.ReadUInt32();

            R.BaseStream.Seek(4, SeekOrigin.Current);
            meshInfo.MeshOffsets             = MeshInfoOffsets.Read(R);
            meshInfo.MeshOffsets2            = MeshInfoOffsets.Read(R);
            meshInfo.TransparentMeshOffsets  = MeshInfoOffsets.Read(R);

            meshInfo.MeshOffsets.MatRanges            = MatIdOffset;
            meshInfo.MeshOffsets2.MatRanges           = MatIdOffset2;
            meshInfo.TransparentMeshOffsets.MatRanges = MatIdOffset3;

            R.BaseStream.Seek(30, SeekOrigin.Current);

            return meshInfo;
        }
    }

    public struct MeshInfoOffsets
    {
        public static uint SIZE = 38;

        public uint MatRanges;
        public uint Offset;
        public uint Verts;
        public uint Uvs;
        public uint Normals;
        public uint VertexColors;

        public uint NumVerts
        {
            get
            {
                var sizeInBytes     = Uvs - Verts;
                var numVerts        = sizeInBytes / Vertex.SIZE;

                return numVerts;
            }
        }

        public static MeshInfoOffsets Read(BinaryReader R)
        {
            var offsets = new MeshInfoOffsets();

            offsets.Offset             = R.ReadUInt32();
            offsets.Verts              = R.ReadUInt32();
            offsets.Uvs                = R.ReadUInt32();
            offsets.VertexColors       = R.ReadUInt32();
            offsets.Normals            = R.ReadUInt32();

            return offsets;
        }
    }
}
