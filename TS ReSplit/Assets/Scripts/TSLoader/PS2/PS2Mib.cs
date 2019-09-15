using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PS2.Vag;

namespace PS2
{
    // Pretty much a header less Sony VAG file used for music in TS2
    // TODO: Come back and tidy this up
    public class Mib
    {
        public static Meta TS2MusicDefault = new Meta() { NumChannels = 2, Frequency = 44100, Interleave = 0x6740 };

        public DecodeMode Mode { get; private set; }
        public Meta MetaInfo { get; set; }
        public AudioBlock[] AudioBlocks;
        public float[] Samples;
        public int BlocksPerInterleave { get { return Interleave / (int)AudioBlock.SIZE; } }
        private int BytesPerInterleave = 0;
        private int CurrentBlockIdx    = 0;
        private int Interleave = 0;

        // Temp buffers for decoding later
        private float[] TempSamplesBuff      = null;
        private short[][] TempPrevSamples    = null;
        private int[] TempBufferOffset       = null;
        private int[] TempBlockChIdx         = null;

        private short[] PreviousSamples = new short[2];

        public Mib()
        {
            Mode    = DecodeMode.DecodeLater;
        }

        public Mib(byte[] Data, DecodeMode Mode = DecodeMode.DecodeOnLoad, Meta? MetaInfo = null) : this()
        {
            if (MetaInfo == null)
            {
                this.MetaInfo = TS2MusicDefault;
            }

            this.Mode = Mode;
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
            switch (Mode)
            {
                case DecodeMode.DecodeOnLoad:
                    LoadAndDecodeBlocks(R);
                    break;
                case DecodeMode.DecodeLater:
                    LoadBlocks(R);
                    break;
                default:
                    break;
            }
        }

        public float[] GetSamples()
        {
            if (Mode == DecodeMode.DecodeOnLoad)
            {
                return Samples;
            }
            else if (Mode == DecodeMode.DecodeLater)
            {
                return DecodeSamples();
            }

            return null;
        }

        public int GetNumSamples()
        {
            var numSamples = AudioBlocks.Length * AudioBlock.SAMPLES_PER_BLOCK;
            return numSamples;
        }

        private void LoadBlocks(BinaryReader R)
        {
            TempSamplesBuff  = new float[AudioBlock.SAMPLES_PER_BLOCK];
            TempPrevSamples  = new short[MetaInfo.NumChannels][];
            TempBufferOffset = new int[MetaInfo.NumChannels];
            TempBlockChIdx   = new int[MetaInfo.NumChannels];

            for (int i = 0; i < MetaInfo.NumChannels; i++)
            {
                TempPrevSamples[i]  = new short[] { 0, 0 };
                TempBufferOffset[i] = i;
                TempBlockChIdx[i]   = 0;
            }

            var numBlocks = R.BaseStream.Length / AudioBlock.SIZE;
            AudioBlocks   = new AudioBlock[numBlocks];

            for (int i = 0; i < AudioBlocks.Length; i++)
            {
                AudioBlocks[i] = AudioBlock.Read(R);
            }

            Interleave = FindInterleaveBytes();
            Debug.WriteLine($"interleave: {Interleave}");
        }

        private void LoadAndDecodeBlocks(BinaryReader R)
        {
            var numBlocks           = R.BaseStream.Length / AudioBlock.SIZE;
            int samplesOffset       = 0;
            var numSamples          = numBlocks * AudioBlock.SAMPLES_PER_BLOCK;
            Samples                 = new float[numSamples];
            var prevSamples         = new short[MetaInfo.NumChannels][];
            var localSmaples        = new float[MetaInfo.NumChannels][];
            var localSamplesOffset  = new int[MetaInfo.NumChannels];
            var interLeaveNumBlocks = 26432 / (int)AudioBlock.SIZE;

            var testSamples = new float[] { 0.0f, 0.0f };

            int outSamplesOffset = 0;

            // Create the temp arrays
            for (int i = 0; i < MetaInfo.NumChannels; i++)
            {
                prevSamples[i]        = new short[] { 0, 0 };
                localSmaples[i]       = new float[interLeaveNumBlocks * AudioBlock.SAMPLES_PER_BLOCK];
                localSamplesOffset[i] = 0;
            }

            var numLoops = (numBlocks / interLeaveNumBlocks) / 2;
            for (int i = 0; i < numLoops; i++)
            {
                for (int chanIdx = 0; chanIdx < MetaInfo.NumChannels; chanIdx++)
                {
                    localSamplesOffset[chanIdx] = 0;

                    for (int aye = 0; aye < interLeaveNumBlocks; aye++)
                    {
                        var block             = AudioBlock.Read(R);
                        var numDecodedSamples = block.Decode(ref localSmaples[chanIdx], localSamplesOffset[chanIdx], ref prevSamples[0]);
                        localSamplesOffset[chanIdx] += numDecodedSamples;
                    }
                }

                try
                {
                    //for (int chanIdx = 0; chanIdx < MetaInfo.NumChannels; chanIdx++)
                    {
                        for (int sampleIdx = 0; sampleIdx < localSmaples[0].Length * 2; sampleIdx += 2)
                        {
                            var localIdx = sampleIdx / 2;
                            var sample1 = localSmaples[0][localIdx];
                            var sample2 = localSmaples[1][localIdx];

                            Samples[outSamplesOffset] = sample1;
                            Samples[outSamplesOffset + 1] = sample2;
                            outSamplesOffset += 2;
                        }
                    }
                }
                catch { }
            }
        }

        public void DecodeInterleaveBlock(BinaryReader R, ref float[] Samples)
        {
            var numBlocksPerInterleave = Interleave / AudioBlock.SIZE;
            for (int i = 0; i < MetaInfo.NumChannels; i++)
            {
                TempBufferOffset[i] = i;
            }

            for (int ch = 0; ch < MetaInfo.NumChannels; ch++)
            {
                for (int blockIdx = 0; blockIdx < numBlocksPerInterleave; blockIdx++)
                {
                    if (CurrentBlockIdx < AudioBlocks.Length)
                    {
                        //var block      = AudioBlock.Read(R);
                        var block        = AudioBlocks[CurrentBlockIdx++];
                        var numDecoded   = block.Decode(ref TempSamplesBuff, 0, ref TempPrevSamples[ch]);

                        for (int sampleIdx = 0; sampleIdx < AudioBlock.SAMPLES_PER_BLOCK; sampleIdx++)
                        {
                            Samples[TempBufferOffset[ch]] = TempSamplesBuff[sampleIdx];
                            TempBufferOffset[ch] += MetaInfo.NumChannels;
                        }
                    }
                    else
                    {
                        Samples = null;
                        return;
                    }
                }
            }
        }

        public void DecodeInterleaveSamples(int NumSamples, ref float[] Samples)
        {
            var samplesPerCh = NumSamples / MetaInfo.NumChannels;
            for (int i = 0; i < MetaInfo.NumChannels; i++)
            {
                TempBufferOffset[i] = i;
            }

            for (int i = 0; i < samplesPerCh; i++)
            {
                for (int ch = 0; ch < MetaInfo.NumChannels; ch++)
                {
                    var block = AudioBlocks[CurrentBlockIdx++];
                }
            }
        }

        private float[] DecodeSamples()
        {
            var numBlocks     = AudioBlocks.Length / AudioBlock.SIZE;
            var samples       = new float[numBlocks];
            int samplesOffset = 0;
            for (int i = 0; i < numBlocks; i++)
            {
                var block      = AudioBlocks[i];
                var numSamples = block.Decode(ref samples, samplesOffset, ref PreviousSamples);
                samplesOffset += numSamples;
            }

            return samples;
        }

        // Nice and conscise name
        public int GetSamplePerInterleaveChannels()
        {
            var samplesPerInterleave = (int)((Interleave / AudioBlock.SIZE) * AudioBlock.SAMPLES_PER_BLOCK);
            var numSamples           = samplesPerInterleave * MetaInfo.NumChannels;
            return numSamples;
        }

        // Becasue this is a headerless file (yay) have to work out what the channel interleave is
        private int FindInterleaveBytes()
        {
            var lastBlockWasntZero = false;
            for (int i = 0; i < AudioBlocks.Length; i++)
            {
                var block  = AudioBlocks[i];
                var isZero = true;
                for (int aye = 0; aye < AudioBlock.SAMPLES_PER_BLOCK / 2; aye++)
                {
                    if (block.Data[aye] != 0)
                    {
                        isZero = false;
                        break;
                    }
                }

                if (lastBlockWasntZero && isZero)
                {
                    var interleave = i * (int)AudioBlock.SIZE;
                    return interleave;
                }

                if (!isZero)
                {
                    lastBlockWasntZero = true;
                }
            }

            return -1;
        }

        public struct Meta
        {
            public int NumChannels;
            public int Frequency;
            public int Interleave;
        }

        public enum DecodeMode
        {
            DecodeOnLoad, // Decodes into float samples at load, doesn't store the compressed audio blocks
            DecodeLater // Loads the audio blocks and stores them, decodes later
        }
    }
}
