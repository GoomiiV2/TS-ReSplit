using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IngameDebugConsole;
using UnityEngine.SceneManagement;

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

    [ConsoleMethod("audio.test", "Test audio file decoding / playback")]
    public static void audio_test()
    {
        var testFilePath = "ts2\\music\\level1\\2025.VAG";
        var data         = TSAssetManager.LoadFile(testFilePath);
        var audioFile    = new PS2.Vag(data);

        var soundSamples = audioFile.GetSamples();
        var clip         = AudioClip.Create("test", soundSamples.Length, audioFile.Head.Channels == 0 ? 1 : audioFile.Head.Channels, (int)audioFile.Head.Frequency, false);
        clip.SetData(soundSamples, 0);

        AudioSource.PlayClipAtPoint(clip, new Vector3(0, 0, 0));
    }

    [ConsoleMethod("audio.testMusic", "Test audio musicfile decoding / playback")]
    public static void audio_test_music()
    {
        var testFilePath = "ts2\\music\\music0\\1001.MIB";
        var data         = TSAssetManager.LoadFile(testFilePath);
        var numBlocks    = data.Length / PS2.Vag.AudioBlock.SIZE;
        var blocks       = new List<PS2.Vag.AudioBlock>((int)numBlocks);

        var audioFile         = new PS2.Vag();
        audioFile.AudioBlocks = blocks.ToArray();

        var soundSamples = audioFile.GetSamples();
        var clip         = AudioClip.Create("test", soundSamples.Length, 1, 44100, false);
        clip.SetData(soundSamples, 0);

        AudioSource.PlayClipAtPoint(clip, new Vector3(0, 0, 0));
    }
}
