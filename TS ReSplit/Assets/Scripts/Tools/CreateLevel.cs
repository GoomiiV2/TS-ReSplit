using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class CreateLevel : EditorWindow
{
    private const string GEN_BASE_OBJ_NAME      = "Level Base";
    private const string LEVEL_MANAGER_OBJ_NAME = "LevelManager";
    private const string SECTION_BASE_OBJ_NAME  = "Sections";

    private string PakPath                      = "ts2/pak/story/l_35_ST.pak";
    private int LevelID                         = 35;

    static CreateLevel()
    {
        EditorSceneManager.sceneSaving         += BeforeSaving;
        EditorSceneManager.sceneSaved          += AfterSaving;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange Obj)
    {
        if (Obj == PlayModeStateChange.ExitingPlayMode)
        {
            RemoveGenratedContent();
            Debug.Log("Exited play mode, deleting genrated data");
        }
    }

    private static void AfterSaving(UnityEngine.SceneManagement.Scene Scene)
    {
        SetHideFlagsForGenratedObjects(Scene, HideFlags.None);
    }

    private static void BeforeSaving(UnityEngine.SceneManagement.Scene Scene, string Path)
    {
        // Set the genrated data to not save as we only want it to be loaded and built from the games origanl data
        // but to bake lightmaps in the editor unity doesn't like this being set, so I do it here before saving :>
        SetHideFlagsForGenratedObjects(Scene, HideFlags.DontSaveInEditor);
    }

    private static void SetHideFlagsForGenratedObjects(UnityEngine.SceneManagement.Scene Scene, HideFlags HideFlag)
    {
        var rootObjs = Scene.GetRootGameObjects();

        foreach (var obj in rootObjs)
        {
            if (obj.name == "Level Base")
            {
                obj.hideFlags = HideFlag;
            }
        }

        Debug.Log($"Set Level Base HideFlags to {HideFlag}");
    }

    [MenuItem(ReSplitMenus.MenuName + "/Create Level")]
    static void Init()
    {
        CreateLevel window = GetWindow<CreateLevel>();
        window.title = "Create Level";
        EditorPrefs.SetBool("MeshRendererEditor.Lighting.ShowSettings", false);

        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Level Pak:", GUILayout.Width(80));
            PakPath = GUILayout.TextField(PakPath);
            int.TryParse(GUILayout.TextField($"{LevelID}", GUILayout.Width(40)), out LevelID);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Create Level")) { CreateNewLevel(); }

        if (GUILayout.Button("Rebuild Level")) { RebuildLevel(); }

        if (GUILayout.Button("Apply Baked Data")) { ApplyBakedData(); }

        if (GUILayout.Button("Save Baked Data")) { SaveBakedData(); }
    }

    void CreateNewLevel()
    {
        WipeScene();

        var levelManagerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/LevelManager.prefab", typeof(GameObject));
        var spawnedGO          = PrefabUtility.InstantiatePrefab(levelManagerPrefab) as GameObject;
        var manager            = spawnedGO.GetComponent<TS2Level>();
        manager.LevelID        = $"{LevelID}";
        manager.LevelPak       = PakPath;

        //manager.Start();
    }

    void RebuildLevel()
    {
        var levelManager = GameObject.Find(LEVEL_MANAGER_OBJ_NAME).GetComponent<TS2Level>();
        RemoveGenratedContent();

        levelManager.Start();
    }

    void ApplyBakedData()
    {
        var levelManager  = GameObject.Find(LEVEL_MANAGER_OBJ_NAME).GetComponent<TS2Level>();
        var levelBase = GameObject.Find(GEN_BASE_OBJ_NAME);
        var levelSections = levelBase.transform.Find(SECTION_BASE_OBJ_NAME).transform;

        Debug.Log($"Applying baked scene data...");
        for (int i = 0; i < levelSections.childCount; i++)
        {
            var section      = levelSections.GetChild(i).gameObject;
            var bakedSection = levelManager.BakedData.PerSectionData[i];

            bakedSection.Apply(section);

            Debug.Log($"Applied baked data to {section.name} index: {i}");
        }
        Debug.Log($"Baked scene data applyied");
    }

    void SaveBakedData()
    {
        var levelManager  = GameObject.Find(LEVEL_MANAGER_OBJ_NAME).GetComponent<TS2Level>();
        var levelBase     = GameObject.Find(GEN_BASE_OBJ_NAME);
        var levelSections = levelBase.transform.Find(SECTION_BASE_OBJ_NAME).transform;

        levelManager.BakedData.PerSectionData = new List<SectionBakedData>();

        for (int i = 0; i < levelSections.childCount; i++)
        {
            var section      = levelSections.GetChild(i).gameObject;
            var bakedSection = SectionBakedData.CreateFromGameObject(section);

            levelManager.BakedData.PerSectionData.Add(bakedSection);
        }

        EditorSceneManager.MarkSceneDirty(levelManager.gameObject.scene);

        Debug.Log("Saved baked data :>");
    }

    public static void RemoveGenratedContent()
    {
        var levelBase = GameObject.Find(GEN_BASE_OBJ_NAME);

        if (levelBase != null)
        {
            DestroyImmediate(levelBase);
            RemoveGenratedContent(); // Recursive incase their are multiple
        }
    }

    private void WipeScene()
    {
        /*var objects = GameObject.all
        foreach (var obj in objects)
        {
            GameObject.DestroyImmediate(obj);
        }*/

    }
}
