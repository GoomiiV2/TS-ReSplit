using System.IO;

namespace TS2
{
    public struct Vertex
    {
        public const uint SIZE = 16;

        public float X;
        public float Y;
        public float Z;
        public byte SameStrip;
        public byte Flag;
        public uint Scale;

        public static Vertex Read(BinaryReader R)
        {
            var vert = new Vertex()
            {
                X         = R.ReadSingle(),
                Y         = R.ReadSingle(),
                Z         = R.ReadSingle(),
                SameStrip = R.ReadByte(),
                Flag      = R.ReadByte(),
                Scale     = R.ReadUInt16()
            };

            return vert;
        }
    }
}
