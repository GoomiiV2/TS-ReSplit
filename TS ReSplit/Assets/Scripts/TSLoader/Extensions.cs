using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public static uint ReadUint32BE(this BinaryReader R)
        {
            var numInBytes = R.ReadBytes(4);
            var num        = ToInt32BigEndian(numInBytes);
            return num;
        }

        public static uint ToInt32BigEndian(byte[] buf)
        {
            return (uint)((buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3]);
        }

        // Taken from https://stackoverflow.com/a/34180417
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            GeneralPropertyComparer<T, TKey> comparer = new GeneralPropertyComparer<T, TKey>(property);
            return items.Distinct(comparer);
        }
    }

    // Taken from https://stackoverflow.com/a/34180417
    public class GeneralPropertyComparer<T, TKey> : IEqualityComparer<T>
    {
        private Func<T, TKey> expr { get; set; }
        public GeneralPropertyComparer(Func<T, TKey> expr)
        {
            this.expr = expr;
        }

        public bool Equals(T left, T right)
        {
            var leftProp = expr.Invoke(left);
            var rightProp = expr.Invoke(right);
            if (leftProp == null && rightProp == null)
                return true;
            else if (leftProp == null ^ rightProp == null)
                return false;
            else
                return leftProp.Equals(rightProp);
        }

        public int GetHashCode(T obj)
        {
            var prop = expr.Invoke(obj);
            return (prop == null) ? 0 : prop.GetHashCode();
        }
    }
}
