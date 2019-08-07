using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS;

namespace PS2
{
    public class Vag
    {
        public Header Head;
        public AudioBlock[] AudioBlocks;

        public Vag() { }

        public Vag(byte[] Data)
        {
            Load(Data);
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
            Head = Header.Read(R);
            LoadBlocks(R);
        }

        public void LoadBlocks(BinaryReader R)
        {
            var numBlocks = Head.DataSize / AudioBlock.SIZE;
            AudioBlocks   = new AudioBlock[numBlocks];

            for (int i = 0; i < AudioBlocks.Length; i++)
            {
                AudioBlocks[i] = AudioBlock.Read(R);
            }
        }


        public struct Header
        {
            public const uint SIZE = 30;

            public char[] Magic; // 4 bytes
            public uint Version;
            public uint DataSize;
            public uint Frequency;
            public byte Channels;
            public string Name; // 10 bytes

            public static Header Read(BinaryReader R)
            {
                var data = new Header();

                data.Magic   = R.ReadChars(4);
                data.Version = R.ReadUint32BE();
                R.BaseStream.Seek(4, SeekOrigin.Current);

                data.DataSize  = R.ReadUint32BE();
                data.Frequency = R.ReadUint32BE();
                R.BaseStream.Seek(0x0A, SeekOrigin.Current);

                data.Channels = R.ReadByte();
                R.BaseStream.Seek(1, SeekOrigin.Current);

                data.Name = new string(R.ReadChars(0x10)).Trim();

                return data;
            }
        }

        public struct AudioBlock
        {
            public const uint SIZE = 16;

            public byte DecodingCoeff;
            public LoopType Type;
            public byte[] Data;

            public static AudioBlock Read(BinaryReader R)
            {
                var block = new AudioBlock();

                block.DecodingCoeff = R.ReadByte();
                block.Type          = (LoopType)R.ReadByte();
                block.Data          = R.ReadBytes(14);

                return block;
            }

            public enum LoopType : byte
            {
                Default     = 0,
                LastBlock   = 1,
                LoopEnd     = 3,
                LoopStart   = 6,
                PlaybackEnd =7
            }
        }
    }
}

