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

        public void Load(byte[] Data)
        {
            using (BinaryReader r = new BinaryReader(new MemoryStream(Data)))
            {
                ID     = r.ReadUInt32();
                UNK    = r.ReadUInt32();
                Width  = r.ReadInt32();
                Height = r.ReadInt32();

                Palettle  = new uint[256];
                var bytes = r.ReadBytes(sizeof(uint) * Palettle.Length);
                Buffer.BlockCopy(bytes, 0, Palettle, 0, bytes.Length);

                var numPixels = (int)(Width * Height);
                Pixels        = r.ReadBytes(numPixels);

                Meta.HasAlpha = ScanForAlpha();
            }
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
