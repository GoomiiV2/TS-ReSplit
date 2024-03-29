﻿using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Unity.Profiling;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

// Steam decode a mib file as an audio clip, data isn't streamed from disk but from a compressed buffer
// TODO: Fix the poping sound that I assume is from decoding a interleave block taking too long
public class StreamedMib
{
    public  PS2.Mib                     MibFile { get; private set; } = null;
    public  AudioClip                   Clip    { get; private set; }
    private BinaryReader                Reader = null;
    private AudioClip.PCMReaderCallback ReadCB;
    private float[][]                   ActiveSamples    = null; // Two buffers
    private int                         ActiveBufferIdx  = 0;
    private int                         ActiveSamplesPos = 0;

    static ProfilerMarker ProfileMarkerDecode      = new ProfilerMarker("ReSplit.Audio.DecodeSample");
    static ProfilerMarker ProfileMarkerPCMCallback = new ProfilerMarker("ReSplit.Audio.PCMCallback");

    public StreamedMib(string FilePath, string Name = "Streamed Mib Audio")
    {
        var data = TSAssetManager.LoadFile(FilePath);
        Create(data, Name);
    }

    public StreamedMib(byte[] data, string Name = "Streamed Mib Audio")
    {
        Create(data, Name);
    }

    public void Create(byte[] data, string Name = "Streamed Mib Audio")
    {
        Reader = new BinaryReader(new MemoryStream(data));

        MibFile = new PS2.Mib(data, PS2.Mib.DecodeMode.DecodeLater);

        var samplesPerInterleave = MibFile.GetSamplePerInterleaveChannels();
        var numSamples           = samplesPerInterleave;
        ActiveSamples    = new float[2][];
        ActiveSamples[0] = new float[numSamples];
        ActiveSamples[1] = new float[numSamples];

        // Unity will call back looking for some samples to play, trouble is we have iteleaved blocks in the MIB file that vary and need to be re interleaved
        // So we decode a whole interleave block and interleave them as LRLRLRLR and buffer it to feed to Unity as it wants
        // and read a new interleave block when we have used all the currently buffered ones
        MibFile.DecodeInterleaveBlock(ref ActiveSamples[0]);
        MibFile.DecodeInterleaveBlock(ref ActiveSamples[1]);

        ReadCB = delegate(float[] Samples)
        {
            ProfileMarkerPCMCallback.Begin();
            if (ActiveSamples[0] == null || ActiveSamples[1] == null) {
                Samples = null;
            }
            else {
                var samplesLeftInBuff = numSamples - ActiveSamplesPos;
                if (samplesLeftInBuff > Samples.Length) {
                    Array.Copy(ActiveSamples[ActiveBufferIdx], ActiveSamplesPos, Samples, 0, Samples.Length);
                    ActiveSamplesPos += Samples.Length;
                    //Debug.Log("copying audio data");
                }
                else {
                    var buffIdx            = ActiveBufferIdx == 0 ? 1 : 0;
                    var samplesInOtherBuff = Samples.Length - samplesLeftInBuff;
                    Array.Copy(ActiveSamples[ActiveBufferIdx], ActiveSamplesPos, Samples, 0, samplesLeftInBuff);
                    Array.Copy(ActiveSamples[buffIdx], 0, Samples, 0, samplesInOtherBuff);

                    // Read in the next block
                    ProfileMarkerDecode.Begin();

                    var task = Task.Factory.StartNew(x =>
                    {
                        MibFile.DecodeInterleaveBlock(ref ActiveSamples[(int)x]);
                    }, ActiveBufferIdx);
                    
                    ProfileMarkerDecode.End();
                    //Debug.Log($"pcm callback called, samples lenght: {Samples.Length}");

                    ActiveBufferIdx  = buffIdx;
                    ActiveSamplesPos = samplesInOtherBuff;
                }
            }

            ProfileMarkerPCMCallback.End();
        };

        Clip = AudioClip.Create(Name, MibFile.GetNumSamples(), MibFile.MetaInfo.NumChannels, MibFile.MetaInfo.Frequency, true, ReadCB);
    }
}