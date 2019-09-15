using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PS2
{
    public class SndUtils
    {
        // Loads a MIB music file and dumps the samples to a file
        public static void SaveMIBToFile(string MibFilePath, string OutPath)
        {
            var data      = TSAssetManager.LoadFile(MibFilePath);
            var musicFile = new PS2.Mib(data);
            var samples   = musicFile.GetSamples();

            var bytes = new byte[samples.Length * 4];
            Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);

            File.WriteAllBytes(OutPath, bytes);
        }

        // Loads a vag file and dumps the samples to a file
        public static void SaveVagToFile(string VagFile, string OutPath)
        {
            var data    = TSAssetManager.LoadFile(VagFile);
            var vagFile = new PS2.Vag(data);
            var samples = vagFile.GetSamples();

            var bytes = new byte[samples.Length * 4];
            Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);

            File.WriteAllBytes(OutPath, bytes);
        }

        /*public static AudioClip CreateStreamingACFromMib()
        {

        }*/
    }
}
