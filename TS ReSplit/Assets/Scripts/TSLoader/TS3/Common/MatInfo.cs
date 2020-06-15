using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3
{
    public struct MatInfo
    {
        int TexID;
        int TexID2;
        int UNK;
        int UNK2;

        public static MatInfo[] ReadMatInfos(BinaryReaderEndian R, int Offset)
        {
            R.BaseStream.Seek(Offset, SeekOrigin.Begin);
            var materials = new List<MatInfo>();

            while (true)
            {
                var matInfo = Read(R);

                if ((uint)matInfo.TexID != 0xFFFFFFFF)
                {
                    materials.Add(matInfo);
                }
                else { break; }
            }

            return materials.ToArray();
        }

        public static MatInfo Read(BinaryReaderEndian R)
        {
            var matInfo = new MatInfo()
            {
                TexID  = R.ReadInt32(),
                TexID2 = R.ReadInt32(),
                UNK    = R.ReadInt32(),
                UNK2   = R.ReadInt32()
            };

            return matInfo;
        }
    }
}
