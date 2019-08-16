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
        private const int SAMPLES_PER_BLOCK = 28; // 14 bytes with a nibble being a sample

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
            var numSamples     = AudioBlocks.Length * SAMPLES_PER_BLOCK;
            var samples        = new float[numSamples];
            var sampleIdx      = 0;
            double prevSample1 = 0.0f;
            double prevSample2 = 0.0f;

            for (int i = 0; i < AudioBlocks.Length; i++)
            {
                var block        = AudioBlocks[i];
                var predictor    = block.DecodingCoeff >> 4;
                var shift        = block.DecodingCoeff & 0xF;
                var localSamples = new double[SAMPLES_PER_BLOCK];

                int byteIdx = 0;
                for (int aye = 0; aye < SAMPLES_PER_BLOCK; aye += 2)
                {
                    byte delta  = block.Data[byteIdx++];
                    var sample = (short)((delta & 0xf) << 12);

                    if ((sample & 0x8000) == 1) { sample |= unchecked((short)0xffff0000); }
                    localSamples[aye] = (sample >> shift);

                    sample = (short)((delta & 0xf0) << 8);

                    if ((sample & 0x8000) == 1) { sample |= unchecked((short)0xffff0000); }
                    localSamples[aye + 1] = (sample >> shift);

                }

                // TODO: see if I can do with out this second loop
                for (int aye = 0; aye < SAMPLES_PER_BLOCK; aye++)
                {
                    localSamples[aye] = localSamples[aye] + prevSample1 * AudioBlock.PredictorLookup[predictor][0] + prevSample2 * AudioBlock.PredictorLookup[predictor][1];
                    prevSample2       = prevSample1;
                    prevSample1       = localSamples[aye];

                    samples[sampleIdx++] = (float)(localSamples[aye]) / (float)32768;
                }
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
            public const uint SIZE = 16;

            public byte DecodingCoeff;
            public LoopType Type;
            public byte[] Data;

            public static float[] LastSamples = new float[] { 0.0f, 0.0f };

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
                PlaybackEnd = 7
            }

            #region Lookup Tables for ADPCM
            public static readonly double[][] PredictorLookup = new double[][] {
                new double[] {0.0f, 0.0f},
                new double[] { 60.0f / 64.0f, 0.0f},
                new double[] {115.0f / 64.0f, -52.0f / 64.0f},
                new double[] {98.0f / 64.0f, -55.0f / 64.0f},
                new double[] {122.0f / 64.0f, -60.0f / 64.0f}
            };
            #endregion
        }
    }
}

