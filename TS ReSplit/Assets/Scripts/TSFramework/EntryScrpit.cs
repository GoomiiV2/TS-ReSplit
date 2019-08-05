﻿using Assets.Scripts.TSFramework;
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

        SceneManager.LoadScene("Assets/Scenes/TS2/Exploring/LevelExplorer.unity", LoadSceneMode.Single);
    }

    private void OnGUI()
    {
        Utils.ProjectLogo();
    }
}
