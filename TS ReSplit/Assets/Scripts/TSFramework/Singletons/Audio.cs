using Assets.Scripts.TSFramework.Singletons;
using System.IO;
using UnityEngine;

namespace TSFramework.Singletons
{
    public class Audio
    {
        // Convert an sound file into an audio clip
        public AudioClip GetAudioClip(string FilePath)
        {
            var audioClip = ReSplit.Cache.TryCache(FilePath, (path) =>
            {
                var name      = Path.GetFileNameWithoutExtension(path);
                var data      = TSAssetManager.LoadFile(path);
                var audioFile = new PS2.Vag(data);

                var soundSamples = audioFile.GetSamples();
                var clip         = AudioClip.Create(name, soundSamples.Length, audioFile.Head.Channels == 0 ? 1 : audioFile.Head.Channels, (int)audioFile.Head.Frequency, false);
                clip.SetData(soundSamples, 0);

                return clip;
            }, CacheType.ClearOnLevelLoad);

            return audioClip;
        }
    }
}
