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
        public uint MatIdOffset;
        public uint Offset;
        public uint VertsOffset;
        public uint UvsOffset;
        public uint NormalsOffset;
        public uint VertexColorsOffset;

        public uint NumVerts
        {
            get
            {
                const int VERT_SIZE = 16;
                var sizeInBytes     = UvsOffset - VertsOffset;
                var numVerts        = sizeInBytes / VERT_SIZE;

                return numVerts;
            }
        }

        public static MeshInfo Read(BinaryReader R)
        {
            var meshInfo = new MeshInfo();

            R.BaseStream.Seek(3, SeekOrigin.Current);
            meshInfo.ID = R.ReadByte();

            R.BaseStream.Seek(16, SeekOrigin.Current);
            meshInfo.MatIdOffset = R.ReadUInt32();

            R.BaseStream.Seek(12, SeekOrigin.Current);
            meshInfo.Offset             = R.ReadUInt32();
            meshInfo.VertsOffset        = R.ReadUInt32();
            meshInfo.UvsOffset          = R.ReadUInt32();
            meshInfo.VertexColorsOffset = R.ReadUInt32();
            meshInfo.NormalsOffset      = R.ReadUInt32();

            R.BaseStream.Seek(88, SeekOrigin.Current);

            return meshInfo;
        }
    }
}
