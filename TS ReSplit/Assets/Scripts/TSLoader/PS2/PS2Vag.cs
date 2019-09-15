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

        public float[] GetSamples()
        {
            var numSamples  = AudioBlocks.Length * AudioBlock.SAMPLES_PER_BLOCK;
            var samples     = new float[numSamples];
            var sampleIdx   = 0;
            var prevSamples = new short[] { 0, 0 };

            for (int i = 0; i < AudioBlocks.Length; i++)
            {
                var block  = AudioBlocks[i];
                sampleIdx += block.Decode(ref samples, sampleIdx, ref prevSamples);
            }

            return samples;
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
            public const int SAMPLES_PER_BLOCK = 28; // 14 bytes with a nibble being a sample
            public const uint SIZE             = 16;

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

            // Decode this block into an existing buffer at a given index
            public int Decode(ref float[] Buffer, int BufferOffset, ref short[] PrevSamples)
            {
                var predictor = DecodingCoeff >> 4;
                var shift     = DecodingCoeff >> 0 & 0xF;

                int byteIdx = 0;
                for (int aye = 0; aye < SAMPLES_PER_BLOCK; aye += 2)
                {
                    byte delta = Data[byteIdx++];
                    var sample = (short)((delta & 0xf) << 12);

                    if ((sample & 0x8000) == 1) { sample |= unchecked((short)0xffff0000); }
                    Buffer[BufferOffset + aye] = DecodeSample((sample >> shift), predictor, ref PrevSamples);

                    sample = (short)((delta & 0xf0) << 8);

                    if ((sample & 0x8000) == 1) { sample |= unchecked((short)0xffff0000); }
                    Buffer[BufferOffset + aye + 1] = DecodeSample((sample >> shift), predictor, ref PrevSamples);
                }

                return SAMPLES_PER_BLOCK;
            }

            private float DecodeSample(int Sample, int Predictor, ref short[] PrevSamples)
            {
                var samp = Sample + PredictorLookup[Predictor][0] * PrevSamples[0] + PredictorLookup[Predictor][1] * PrevSamples[1];
                var sample     = ClampShort((int)samp) / 32768f;
                PrevSamples[1] = PrevSamples[0];
                PrevSamples[0] = ClampShort((int)samp);
                return sample;
            }

            private short ClampShort(int Val)
            {
                if (Val > 32767)
                {
                    return 32767;
                }
                else if (Val < -32768)
                {
                    return -32768;
                }
                else
                {
                    return (short)Val;
                }
            }

            public enum LoopType : byte
            {
                Default     = 0,
                LastBlock   = 1,
                LoopEnd     = 3,
                LoopStart   = 6,
                PlaybackEnd = 7
            }

            #region Lookup Tables for ADPCM
            public static readonly float[][] PredictorLookup = new float[][] {
                new float[] { 0.0f, 0.0f },
                new float[] { 60.0f / 64.0f, 0.0f },
                new float[] { 115.0f / 64.0f, -52.0f / 64.0f },
                new float[] { 98.0f / 64.0f, -55.0f / 64.0f },
                new float[] { 122.0f / 64.0f, -60.0f / 64.0f }
            };
            #endregion
        }
    }
}

