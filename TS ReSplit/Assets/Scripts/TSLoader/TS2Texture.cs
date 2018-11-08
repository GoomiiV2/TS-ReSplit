using System.IO;

namespace TS2
{
    // Lovely and simple :D
    public class Texture
    {
        public uint ID;
        private uint UNK;
        public int Width;
        public int Height;

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

                Palettle = new uint[256];
                for (int i = 0; i < Palettle.Length; i++)
                {
                    Palettle[i] = r.ReadUInt32();
                }

                var numPixels = (int)(Width * Height);
                Pixels = r.ReadBytes(numPixels);
            }
        }
    }
}
