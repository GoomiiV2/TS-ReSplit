using Assets.Scripts.TSFramework.Singletons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.TSFramework
{
    [RequireComponent(typeof(AudioSource))]
    public class LevelEnviro : MonoBehaviour
    {
        public List<MusicTrack> BGMTracks;

        private AudioSource BGMSource;

        void Start()
        {
            BGMSource = GetComponent<AudioSource>();
            PlayTrack(0);
        }

        public void PlayTrack(int TrackIdx)
        {
            if (BGMTracks != null && TrackIdx >= 0 && TrackIdx < BGMTracks.Count) 
            {
                var clip       = BGMTracks[TrackIdx];
                var mib        = GetTrack(clip.TrackPath);
                BGMSource.clip = mib.Clip;
                BGMSource.loop = clip.Loop;

                BGMSource.Play();
            }
        }

        private StreamedMib GetTrack(string Path)
        {
            var track = ReSplit.Cache.TryCache<StreamedMib>(Path, (path) =>
            {
                return new StreamedMib(Path);
            });

            return track;
        }
    }

    [Serializable]
    public struct MusicTrack
    {
        public string TrackPath;
        public bool Loop;
    }
}