using Assets.Scripts.TSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExplorer : MonoBehaviour
{
    public struct ViewerSettings
    {
        public bool ShowUI;
        public bool IsLoading;
    }

    public TS2Level Level;
    private string CurentLevelPak;
    private string LevelId;
    private ViewerSettings Settings = new ViewerSettings()
    {
        IsLoading = false
    };

    private List<string> LevelPaths = new List<string>();
    private Vector2 sidePanelScrollPos;

    public float LeftUIWidth    = 250;
    public float UIButtonHeight = 5;

    void Start()
    {
        GetLevelList();
    }

    void GetLevelList()
    {
        var paths = new string[] { "ts2/pak/story", "ts2/pak/arcade" };
        LevelPaths.Clear();

        foreach (var dir in paths)
        {
            var files = TSAssetManager.GetFilesInDir(dir, "*.pak");
            LevelPaths.AddRange(files);
        }
    }

    void LoadLevel(string LevelPath)
    {
        Settings.IsLoading = true;

        var pakPath = LevelPath.Replace(TSAssetManager.GetCurrentDataPath(), "").TrimStart('\\').Replace("\\", "/");
        var levelId = Path.GetFileNameWithoutExtension(LevelPath).Split('_')[1];

        Debug.Log($"LevelPath: {LevelPath}, pakPath: {pakPath}, levelId: {levelId}");

        Level.ClearGenratedContent();
        Level.LevelPak = pakPath;
        Level.LevelID  = levelId;
        Level.Start();

        Settings.IsLoading = false;
    }

    void OnGUI()
    {
        var x    = Settings.ShowUI ? LeftUIWidth : 5;
        var y    = Settings.ShowUI ? 0 : 5;
        var text = Settings.ShowUI ? "Hide" : "Show";
        if (GUI.Button(new Rect(x, y, 45, UIButtonHeight), text))
        {
            Settings.ShowUI = !Settings.ShowUI;
        }

        if (Settings.ShowUI)
        {
            DrawUI();
        }

        if (Settings.IsLoading)
        {
            var width  = 400;
            var height = 25;
            var pos    = new Rect((Screen.width / 2 - width / 2), (Screen.height / 2 - height / 2), width, height);
            GUI.Box(pos, "Loading");
        }

        Utils.ProjectLogo();
    }

    void DrawUI()
    {
        var rect = new Rect(0, 0, LeftUIWidth, Screen.height);
        GUI.Box(rect, "");

        var buttonHeight = 20;
        var buttonSpacing = 2;
        var count = LevelPaths.Count;
        var listHeight = count * (buttonHeight + buttonSpacing);
        var sideBarHeight = Screen.height;

        GUILayout.BeginArea(rect);
        sidePanelScrollPos = GUILayout.BeginScrollView(sidePanelScrollPos, new GUILayoutOption[] { GUILayout.Width(LeftUIWidth), GUILayout.Height(sideBarHeight) });
        GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width(LeftUIWidth - 20), GUILayout.Height(sideBarHeight) });

        var startIndex = (int)Math.Floor(sidePanelScrollPos.y / (buttonHeight + buttonSpacing));
        var endIdx = (int)Math.Min(startIndex + (Screen.height / (buttonHeight + buttonSpacing)), count);
        
        GUILayout.Space(startIndex * (buttonHeight + buttonSpacing));

        for (int i = startIndex; i < endIdx; i++)
        {
            var file       = LevelPaths[i];
            var isSelected = CurentLevelPak == file ? "> " : "";
            var name       = Path.GetFileName(file);
            if (GUILayout.Button($"{isSelected}{name}"))
            {
                LoadLevel(file);
            }
        }

        GUILayout.Space((count - endIdx) * (buttonHeight + buttonSpacing));
        
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        // Other viewers
        var viewersSelect = new Rect(Screen.width - 155, 5, 150, 200);
        GUILayout.BeginArea(viewersSelect);
        if (GUILayout.Button($"Model Viewer"))
        {
            SceneManager.LoadScene("Assets/Scenes/TS2/Exploring/AnimationExplorer.unity", LoadSceneMode.Single);
        }
        GUILayout.EndArea();

        DrawLevelSettings();
    }

    void DrawLevelSettings()
    {
        if (Level != null)
        {
            var x = LeftUIWidth + 2;
            var y = 40;

            var showMainMesh      = Level.ImportMainGeo;
            var showMeshOverlays  = Level.ImportOverlayGeo;
            var showDecalOverlays = Level.ImportDecalGeo;

            Level.ImportMainGeo    = GUI.Toggle(new Rect(x, y, 200, 15), Level.ImportMainGeo, "Show Main Mesh");
            Level.ImportOverlayGeo = GUI.Toggle(new Rect(x, y + 15, 200, 15), Level.ImportOverlayGeo, "Show Overlays Mesh");
            Level.ImportDecalGeo   = GUI.Toggle(new Rect(x, y + 30, 200, 15), Level.ImportDecalGeo, "Show Decal Mesh");

            if (Level.ImportMainGeo != showMainMesh
                    || Level.ImportOverlayGeo != showMeshOverlays
                    || Level.ImportDecalGeo != showDecalOverlays)
            {
                Level.ClearGenratedContent();
                Level.Start();
            }

        }
    }
}
