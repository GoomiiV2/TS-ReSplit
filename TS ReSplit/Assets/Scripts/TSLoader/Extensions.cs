using System;
using System.IO;

namespace TS
{
    public static class Extensions
    {
        public static float[] ReadSingleArray(this BinaryReader R, int Num)
        {
            var floats = new float[Num];
            var bytes  = R.ReadBytes(sizeof(float) * Num);
            Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);

            return floats;
        }
    }
}
