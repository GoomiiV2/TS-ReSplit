using Assets.Scripts.TSFramework;
using IngameDebugConsole;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EntryScrpit : MonoBehaviour
{
    void Start()
    {
        var debugConsole = FindObjectOfType<IngameDebugConsole.DebugLogManager>();
        debugConsole.ShowPopup();

        if (!Application.isEditor)
        {
            SceneManager.LoadScene("Assets/Scenes/TS2/Exploring/LevelExplorer.unity", LoadSceneMode.Single);
        }

        ConsoleCommands.audio_test_music();
    }

    private void OnGUI()
    {
        Utils.ProjectLogo();
    }
}
