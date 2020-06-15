using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3
{
    public class Model
    {
        public ModelInfo Info;
        public MatInfo[] Materials;

        public Model(byte[] Data, bool IsLittleEndian = false)
        {
            Load(Data, IsLittleEndian);
        }

        public void Load(byte[] Data, bool IsLittleEndian = false)
        {
            using (BinaryReaderEndian r = new BinaryReaderEndian(new MemoryStream(Data), IsLittleEndian))
            {
                int MatInfoOffset = r.ReadInt32();
                int InfoOffset    = r.ReadInt32();
                int UNKOffset     = r.ReadInt32();

                Materials = MatInfo.ReadMatInfos(r, MatInfoOffset);

                r.BaseStream.Seek(InfoOffset, SeekOrigin.Begin);
                Info = ModelInfo.Read(r);
            }
        }

        public struct ModelInfo
        {
            public int NumSubMeshes;
            public byte[] UNK;
            public float Scale;
            public int UNK2;
            public float[] UNKFloats;
            public int[] UNK3;

            public static ModelInfo Read(BinaryReaderEndian R)
            {
                var modelInfo = new ModelInfo()
                {
                    NumSubMeshes = R.ReadInt32(),
                    UNK          = R.ReadBytes(8 * sizeof(byte)),
                    Scale        = R.ReadSingle(),
                    UNK2         = R.ReadInt32(),
                    UNKFloats    = R.ReadSingles(3),
                    UNK3         = R.ReadInt32s(2)
                };

                return modelInfo;
            }
        }
    }
}
