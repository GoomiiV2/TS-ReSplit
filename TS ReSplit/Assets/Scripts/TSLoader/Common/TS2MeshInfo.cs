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
        public static uint SIZE       = 144;
        public static byte EMPTY_LINK = 0xFF;

        public byte IsBone;
        public byte Unk2;
        public byte ParentIdx;
        public byte ChildIdx;
        public byte Unk4;
        public byte Unk5;
        public MeshInfoOffsets MeshOffsets;
        public MeshInfoOffsets MeshOffsets2;
        public MeshInfoOffsets TransparentMeshOffsets;

        public bool HasChild { get { return ChildIdx != EMPTY_LINK; } }

        public static MeshInfo Read(BinaryReader R)
        {
            var meshInfo = new MeshInfo();

            meshInfo.IsBone     = R.ReadByte();
            meshInfo.Unk2       = R.ReadByte();
            meshInfo.ParentIdx  = R.ReadByte();
            meshInfo.ChildIdx   = R.ReadByte();
            meshInfo.Unk4       = R.ReadByte();
            meshInfo.Unk5       = R.ReadByte();

            R.BaseStream.Seek(14, SeekOrigin.Current);
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

            R.BaseStream.Seek(48, SeekOrigin.Current);

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
