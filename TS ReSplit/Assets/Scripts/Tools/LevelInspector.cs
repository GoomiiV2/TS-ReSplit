using Assets.Scripts.TSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class LevelInspector : EditorWindow
{
    private string PakPath = "ts2/pak/story/l_35_ST.pak";
    private int LevelID = 35;
    private Vector2 TexturesScroll;
    private string LevelDataPath { get { return $"{PakPath}/bg/level{LevelID}/level{LevelID}.raw"; } }
    private string LevelPadPath { get { return $"{PakPath}/pad/data/level{LevelID}.raw"; } }

    private bool StatsToggle    = false;
    private bool TexturesToggle = false;

    private LevelStats Stats;
    private List<TexData> LevelTextureData;
    private List<Texture2D> LevelTextures;

    [MenuItem(ReSplitMenus.MenuName + "/Level Inspector")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        LevelInspector window = GetWindow<LevelInspector>();
        window.title          = "Level Inspector";

        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Level Pak:", GUILayout.Width(80));
            PakPath = GUILayout.TextField(PakPath);
            int.TryParse(GUILayout.TextField($"{LevelID}", GUILayout.Width(40)), out LevelID);
            if (GUILayout.Button("Load", GUILayout.Width(60))) { LoadPak(); }
        }
        EditorGUILayout.EndHorizontal();

        #region Misc Stats
        StatsToggle = EditorGUILayout.Foldout(StatsToggle, "Stats");
        if (StatsToggle)
        {
            GUI.enabled = false;
            EditorGUILayout.IntField("Num Sections", Stats.NumSections);
            EditorGUILayout.IntField("Num Vis Leafs", Stats.NumVisLeafs);
            EditorGUILayout.IntField("Num Toggle Portals", Stats.NumTogglePortals);
            EditorGUILayout.IntField("Num Materials", Stats.NumMaterials);
            EditorGUILayout.IntField("Num Pathing Nodes", Stats.NumPathingNodes);
            EditorGUILayout.IntField("Num Special Nodes", Stats.NumSpecialNodes);
            GUI.enabled = true;
        }
        #endregion

        #region Textures
        TexturesToggle = EditorGUILayout.Foldout(TexturesToggle, "Textures");
        if (TexturesToggle)
        {
            Vector2 ItemSize             = new Vector2(128, 128 + 20);
            int NumTexturesPerLine       = (int)Math.Floor(position.width / ItemSize.x);
            float ItemSpacing            = 4;
            float height                 = (LevelTextureData.Count / NumTexturesPerLine * ItemSize.y) + (LevelTextureData.Count / NumTexturesPerLine * ItemSpacing);

            int lines = LevelTextureData.Count / NumTexturesPerLine;

            TexturesScroll = EditorGUILayout.BeginScrollView(TexturesScroll);
            EditorGUILayout.BeginVertical(GUILayout.Height(height));
            for (int i = 0; i < lines; i++)
            {
                var lineRect = EditorGUILayout.BeginHorizontal();
                {
                    var textures = LevelTextureData.Skip(i * NumTexturesPerLine).Take(NumTexturesPerLine).ToArray();
                    for (int eye = 0; eye < textures.Count(); eye++)
                    {
                        var texData = textures[eye];
                        var x       = lineRect.x + (eye * ItemSize.x) + (eye * ItemSpacing);
                        var y       = lineRect.y + (i * ItemSize.y) + (i * ItemSpacing);

                        EditorGUI.DrawRect(new Rect(x, y, ItemSize.x, ItemSize.y), Color.gray);
                        EditorGUI.LabelField(new Rect(x, y, ItemSize.x, 20), texData.Name);
                        EditorGUI.DrawPreviewTexture(new Rect(x, y + 20, ItemSize.x, ItemSize.y - 20), texData.Tex, null, ScaleMode.ScaleToFit);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        #endregion
    }

    private void LoadPak()
    {
        var mapData = TSAssetManager.LoadFile(LevelDataPath);
        var padData = TSAssetManager.LoadFile(LevelPadPath);

        var level    = new TS2.Map(mapData);
        var levelPad = new TS2.Pathing(padData);

        Stats.NumSections      = level.Sections.Count;
        Stats.NumVisLeafs      = level.VisPortals.Count;
        Stats.NumTogglePortals = level.PortalDoors != null ? level.PortalDoors.Count : 0;
        Stats.NumMaterials     = level.Materials.Length;
        Stats.NumPathingNodes  = levelPad.PathingNodes.Length;
        Stats.NumSpecialNodes  = levelPad.ExtraNodes.Length;

        LoadTextures(level.Materials);
    }

    private void LoadTextures(TS2.MatInfo[] MaterialInfos)
    {
        var modelPakPath = TSAssetManager.GetPakForPath(LevelDataPath).Item1;
        var texPaths     = TSTextureUtils.GetTexturePathsForMats(MaterialInfos);
        texPaths         = texPaths.OrderBy(x => x).ToArray();

        LevelTextureData  = new List<TexData>();
        LevelTextures     = new List<Texture2D>();

        for (int i = 0; i < texPaths.Length; i++)
        {
            var texID   = MaterialInfos[i];
            var texPath = texPaths[i];

            var texData = TSAssetManager.LoadFile($"{modelPakPath}/{texPath}");
            var ts2tex  = new TS2.Texture(texData);
            var tex     = TSTextureUtils.TS2TexToT2D(ts2tex);

            var texInfo = new TexData()
            {
                Name = texPath,
                Tex  = tex
            };

            LevelTextureData.Add(texInfo);
            LevelTextures.Add(tex);
        }
    }

    private struct LevelStats
    {
        public int NumSections;
        public int NumVisLeafs;
        public int NumTogglePortals;
        public int NumPathingNodes;
        public int NumSpecialNodes;
        public int NumMaterials;
    }

    private struct TexData
    {
        public string Name;
        public Texture2D Tex;
    }
}
