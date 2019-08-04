using System;
using System.IO;

namespace TS2
{
    // Lovely and simple :D
    public class Texture
    {
        const byte OPAQUE_ALPHA_VALUE = 127;
        public uint ID;
        private uint UNK;
        public int Width;
        public int Height;
        public MetaInfo Meta;

        public uint[] Palettle;
        public byte[] Pixels;

        public Texture() { }

        public Texture(byte[] Data)
        {
            Load(Data);

        }

        public static Texture Read(BinaryReader R)
        {
            var tex = new Texture();
            tex.Load(R);
            return tex;
        }

        public void Load(byte[] Data)
        {
            using (BinaryReader r = new BinaryReader(new MemoryStream(Data)))
            {
                Load(r);
            }
        }

        public void Load(BinaryReader R)
        {
            ID     = R.ReadUInt32();
            UNK    = R.ReadUInt32();
            Width  = R.ReadInt32();
            Height = R.ReadInt32();

            Palettle  = new uint[256];
            var bytes = R.ReadBytes(sizeof(uint) * Palettle.Length);
            Buffer.BlockCopy(bytes, 0, Palettle, 0, bytes.Length);

            var numPixels = (int)(Width * Height);
            Pixels        = R.ReadBytes(numPixels);

            Meta.HasAlpha = ScanForAlpha();
        }

        public static float RazzaMap(float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        private bool ScanForAlpha()
        {
            for (int i = 0; i < Palettle.Length; i++) {
                var color = Palettle[i];
                var isAlpha = (color & 0xFF000000) < OPAQUE_ALPHA_VALUE;

                if (isAlpha) { return true; }
            }

            return false;
        }

        public struct MetaInfo
        {
            public bool HasAlpha;
        }
    }
}
