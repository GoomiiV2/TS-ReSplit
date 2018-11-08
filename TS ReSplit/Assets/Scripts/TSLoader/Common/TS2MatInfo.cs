using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace TS2
{

    // Used in models and levels
    public struct MatInfo
    {
        public uint ID;
        // More ??

        public static List<MatInfo> ReadMatInfos(BinaryReader R, uint Offset)
        {
            R.BaseStream.Seek(Offset, SeekOrigin.Begin);
            var materials = new List<MatInfo>();

            while (true)
            {
                var matInfo = new MatInfo();
                matInfo.ID = R.ReadUInt32();

                if (matInfo.ID != 0xFFFFFFFF)
                {
                    R.BaseStream.Seek(12, SeekOrigin.Current);
                    materials.Add(matInfo);
                }
                else { break; }
            }

            return materials;
        }
    }
}
