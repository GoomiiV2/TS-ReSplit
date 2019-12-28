using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;
using Assets.Scripts.TSFramework.Singletons;
using Assets.Scripts.TSFramework;

public class ConsoleCommands : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    [ConsoleMethod("test", "Just a test :>")]
    public static void Test()
    {
        Debug.Log("Yay :>");
    }

    [ConsoleMethod("open_ts2_level_viewer", "Open the level viewer for TimeSplitters 2 levels")]
    public static void open_ts2_level_viewer()
    {
        SceneManager.LoadScene("Assets/Scenes/TS2/Exploring/LevelExplorer.unity", LoadSceneMode.Single);
    }

    [ConsoleMethod("open_ts2_model_viewer", "Open the model viewer for TimeSplitters 2 models and animations")]
    public static void open_ts2_model_viewer()
    {
        SceneManager.LoadScene("Assets/Scenes/TS2/Exploring/AnimationExplorer.unity", LoadSceneMode.Single);
    }

    [ConsoleMethod("open_devmap", "Open dev map level")]
    public static void open_devmap()
    {
        SceneManager.LoadScene("Assets/Scenes/Debug/DevTest.unity", LoadSceneMode.Single);
    }

    [ConsoleMethod("open_test", "Open test map")]
    public static void open_test()
    {
        SceneManager.LoadScene("Assets/Scenes/TS2/MP/Ice Station.unity", LoadSceneMode.Single);
    }

    [ConsoleMethod("audio.test", "Test audio file decoding / playback")]
    public static void audio_test()
    {
        var testFilePath = "ts2\\music\\level1\\fem_anna_burford22fire.vag";
        var data         = TSAssetManager.LoadFile(testFilePath);
        var audioFile    = new PS2.Vag(data);

        var soundSamples = audioFile.GetSamples();
        var clip         = AudioClip.Create("test", soundSamples.Length, audioFile.Head.Channels == 0 ? 1 : audioFile.Head.Channels, (int)audioFile.Head.Frequency, false);
        clip.SetData(soundSamples, 0);

        PS2.SndUtils.SaveVagToFile(testFilePath, "sfxFileTest.raw");

        AudioSource.PlayClipAtPoint(clip, new Vector3(0, 0, 0));
    }

    [ConsoleMethod("audio.testMusic", "Test audio musicfile decoding / playback")]
    public static void audio_test_music()
    {
        var testFilePath = "ts2\\music\\music1\\1030.MIB";
        /*var data         = TSAssetManager.LoadFile(testFilePath);
        var musicFile    = new PS2.Mib(data);

        var samples = musicFile.GetSamples();
        var clip    = AudioClip.Create("test", samples.Length, musicFile.MetaInfo.NumChannels, musicFile.MetaInfo.Frequency, false);
        clip.SetData(samples, 0);

        PS2.SndUtils.SaveMIBToFile(testFilePath, "musicTest.raw");

        

        AudioSource.PlayClipAtPoint(clip, new Vector3(0, 0, 0));*/

        var sMib = new StreamedMib(testFilePath);
        AudioSource.PlayClipAtPoint(sMib.Clip, new Vector3(0, 0, 0));

        /*var data = TSAssetManager.LoadFile(testFilePath);
        var r = new BinaryReader(new MemoryStream(data));
        var musicFile = new PS2.Mib()
        {
            MetaInfo = PS2.Mib.TS2MusicDefault
        };
        var samples = new float[musicFile.GetSamplePerInterleaveChannels()];
        musicFile.DecodeInterleaveBlock(r, ref samples);

        var clip = AudioClip.Create("test", samples.Length, musicFile.MetaInfo.NumChannels, musicFile.MetaInfo.Frequency, false);
        clip.SetData(samples, 0);

        //var samples2 = new float[musicFile.GetSamplePerInterleaveChannels()];
        //clip.SetData(samples2, samples2.Length);

        AudioSource.PlayClipAtPoint(clip, new Vector3(0, 0, 0));*/
    }

    [ConsoleMethod("audio.playbgm", "Play the bgm track for this level with the given index")]
    public static void CacheNumCached(int TrackIdx)
    {
        var enviroGO = GameObject.Find("Enviro");
        if (enviroGO != null)
        {
            var enviro = enviroGO.GetComponent<LevelEnviro>();
            enviro.PlayTrack(TrackIdx);
        }
    }

    [ConsoleMethod("cache.num", "How many Items are cached")]
    public static void CacheNumCached()
    {
        var numCached = ReSplit.Cache.GetNumCached();
        Debug.Log($"There are {numCached} items in the cache");
    }

    [ConsoleMethod("cache.clearall", "Clear all items in the cache")]
    public static void CacheClearAll()
    {
        ReSplit.Cache.Clear();
    }
}
