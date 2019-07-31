using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TS2Data;
using UnityEngine;

public class AnimationExplorer : MonoBehaviour
{
    public struct ViewerSettings
    {
        public float CamDist;
        public float CamSens;
        public Vector3 CamOffset;

        public ViewerMode Mode;
        public string CurrentModel;
        public string CurrentAnimation;
        public TS2.Animation Animation;
        public TS2.Model Model;

        public bool AnimUseRootMotion;
        public bool AnimLoop;
    }

    public enum ViewerMode
    {
        Animations,
        Models
    }

    public GameObject PreviewModel;
    public Camera Cam;
    public ViewerSettings Settings = new ViewerSettings()
    {
        CamDist           = 2f,
        CamSens           = 800f,
        Mode              = ViewerMode.Animations,
        AnimUseRootMotion = false,
        AnimLoop          = true
    };

    private float xRot = -30f;
    private float yRot = 0f;

    private int TopBarHeight = 50;
    private int SidebarWidth = 300;
    private Vector2 sidePanelScrollPos;
    private float SidebarListScrollPos = 0;
    private List<string> AnimationFiles = new List<string>();
    private List<(string PakPath, string ModelPath)> ModelPaths = new List<(string, string)>();

    void Start()
    {
        Invoke("DelayiedStart", 0.2f);
        GetAnimationsList();
        GetModelList();
    }

    void DelayiedStart()
    {
        //PositionCam();
    }

    void PositionCam()
    {
        var modelBounds        = PreviewModel.GetComponent<MeshFilter>().mesh.bounds;
        float cameraDistance   = Settings.CamDist; // Constant factor
        Vector3 objectSizes    = modelBounds.max - modelBounds.min;
        float objectSize       = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);
        float cameraView       = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * Cam.fieldOfView); // Visible height 1 meter in front
        float distance         = cameraDistance * objectSize / cameraView; // Combined wanted distance from the object
        distance              += 0.5f * objectSize; // Estimated offset from the center to the outside of the object
        Cam.transform.position = (PreviewModel.transform.position + modelBounds.center) - distance * Cam.transform.forward;
    }

    void GetAnimationsList()
    {
        AnimationFiles.Clear();

        var animationPaks = new string[] { "ts2/pak/anim.pak" };
        foreach (var animPak in animationPaks)
        {
            var files = TSAssetManager.GetFileListForPak(animPak).Select(x => Path.Combine(animPak, x)); // Should do a simple file format check
            AnimationFiles.AddRange(files);
        }
    }

    void GetModelList()
    {
        ModelPaths.Clear();
        var modelPaks = new string[] { "ts2/pak/chr.pak", "ts2/pak/gun.pak" };
        foreach (var modelPak in modelPaks)
        {
            var files = TSAssetManager.GetFileListForPak(modelPak).Select(x => (modelPak, modelPak + "/" + x)).Where(x => !x.Item2.Contains("textures")); // Should do a simple file format check
            ModelPaths.AddRange(files);
        }

        ModelPaths = ModelPaths.OrderBy(x => x.PakPath).ThenBy(x => Path.GetFileName(x.ModelPath)).ToList();
    }

    void PlayAnimation(string Animation)
    {
        Settings.Animation = null;

        var animData   = TSAssetManager.LoadFile(Animation);
        var ts2Anim    = new TS2.Animation(animData);
        var clip       = TSAnimationUtils.ConvertAnimation(ts2Anim, TS2AnimationData.HumanSkel, Animation, UseRootMotion: Settings.AnimUseRootMotion, IsLooping: Settings.AnimLoop);
        Settings.Animation = ts2Anim;

        var aninmation = PreviewModel.GetComponent<Animation>();
        aninmation.RemoveClip(clip);
        aninmation.Stop();
        aninmation.AddClip(clip, clip.name);
        aninmation.Play(clip.name, PlayMode.StopAll);
    }

    void LoadModel(string ModelPath)
    {
        var modelScript = PreviewModel.GetComponent<AnimatedModelV2>();

        int i = 0;
        foreach (var models in ModelDB.Models)
        {
            foreach (var model in models)
            {
                if (model.Value.Path == ModelPath)
                {
                    modelScript.ModelType = (ModelType)i;
                    modelScript.ModelName = model.Value.Name;
                    modelScript.LoadModel();
                    PositionCam();
                    return;
                }
            }

            i++;
        }

        PreviewModel.transform.position = Vector3.zero;
        modelScript.LoadModel(ModelPath, null);

        var modelBounds = PreviewModel.GetComponent<SkinnedMeshRenderer>().bounds;
        if (modelBounds.min.y <= 0)
        {
            PreviewModel.transform.position = new Vector3(0, Math.Abs(modelBounds.min.y), 0);
        }
        PositionCam();

        try
        {
            var modelFileData = TSAssetManager.LoadFile(ModelPath);
            var tS2Model      = new TS2.Model(modelFileData);
            Settings.Model    = tS2Model;
        }
        catch { }
    }

    void Update()
    {
        //PositionCam();

        if (Input.mousePosition.x > SidebarWidth)
        {
            Settings.CamDist -= Input.mouseScrollDelta.y * 0.2f;

            if (Input.GetMouseButton(0))
            {
                xRot += Input.GetAxis("Mouse Y") * Settings.CamSens * Time.deltaTime;
                yRot += Input.GetAxis("Mouse X") * Settings.CamSens * Time.deltaTime;

                if (xRot > 90f)
                {
                    xRot = 90f;
                }
                else if (xRot < -90f)
                {
                    xRot = -90f;
                }
            }

            if (Input.GetMouseButton(1))
            {
                var x = Input.GetAxis("Mouse Y") * Time.deltaTime;
                var y = Input.GetAxis("Mouse X") * Time.deltaTime;
                Settings.CamOffset += new Vector3(0, x, 0);
            }
        }

        var modelBounds        = PreviewModel.GetComponent<MeshFilter>().mesh.bounds;
        Cam.transform.position = (PreviewModel.transform.position + modelBounds.center + Settings.CamOffset) + Quaternion.Euler(xRot, yRot, 0f) * (Settings.CamDist * -Vector3.back);
        Cam.transform.LookAt((PreviewModel.transform.position + modelBounds.center) + Settings.CamOffset, Vector3.up);
    }

    void OnGUI()
    {
        DrawUITopBar();
        DrawSidePanel();
    }

    void DrawUITopBar()
    {
        var rect = new Rect(0, 0, Screen.width, TopBarHeight);
        GUI.Box(rect, "");
        GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Width(Screen.width), GUILayout.MinHeight(TopBarHeight), GUILayout.ExpandHeight(true) });
                GUILayout.BeginHorizontal(new GUILayoutOption[] { GUILayout.Width(SidebarWidth), GUILayout.MinHeight(TopBarHeight), GUILayout.ExpandHeight(true) });
                    if (GUILayout.Button("Animations", new GUILayoutOption[] { GUILayout.ExpandHeight(true) })) { Settings.Mode = ViewerMode.Animations; }
                    if (GUILayout.Button("Models", new GUILayoutOption[] { GUILayout.ExpandHeight(true) })) { Settings.Mode = ViewerMode.Models; }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                DrawAnimationSettings();
                DrawAnimationInfo();

                DrawModelInfo();
            GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void DrawAnimationSettings()
    {
        if (Settings.Mode == ViewerMode.Animations)
        {
            bool useRootMotion = Settings.AnimUseRootMotion;
            bool loop          = Settings.AnimLoop;

            GUILayout.BeginHorizontal(new GUILayoutOption[] { });
                GUILayout.BeginVertical();
                    Settings.AnimUseRootMotion = GUILayout.Toggle(Settings.AnimUseRootMotion, "Root Motion");
                    Settings.AnimLoop          = GUILayout.Toggle(Settings.AnimLoop, "Loop");
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            if (useRootMotion != Settings.AnimUseRootMotion
                || loop != Settings.AnimLoop)
            {
                PlayAnimation(Settings.CurrentAnimation);
            }
        }
    }

    void DrawAnimationInfo()
    {
        if (Settings.Animation != null)
        {
            var an = Settings.Animation;

            GUILayout.BeginHorizontal(new GUILayoutOption[] { });
                GUILayout.BeginVertical();
                    //GUILayout.Label($"Animation Info ({Path.GetFileName(Settings.CurrentAnimation)}):");
                    GUILayout.Label($"{Path.GetFileName(Settings.CurrentAnimation)}: Magic: {new string(an.Head.Magic)} Ver: {an.Head.Version} Unk1: {an.Head.Unk1} Unk2: {an.Head.Unk2}");
                    GUILayout.Label($"Bone Tracks: {an.BoneTracks.Length}, Root Frames: {an.RootFrames.Length}, Frames: {an.Frames.Length}");
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    void DrawModelInfo()
    {
        if (Settings.CurrentModel != null)
        {
            /*var mdl         = Settings.Model;
            var modelScript = PreviewModel.GetComponent<MeshFilter>();
            var triCount    = 0;
            for (int i = 0; i < modelScript.mesh.subMeshCount; i++) { triCount += modelScript.mesh.GetIndices(i).Length; }

            GUILayout.BeginHorizontal(new GUILayoutOption[] { });
            GUILayout.BeginVertical();
            GUILayout.Label($"({Path.GetFileName(Settings.CurrentModel)}): Num Mats: {mdl.Materials.Length}, Num Meshes: {mdl.Meshes.Length}, Num Meshes Infos: {mdl.MeshInfos.Length}");
            GUILayout.Label($"Num Verts: {modelScript.mesh.vertices.Length}, Num Tris: {triCount}");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();*/
        }
    }

    void DrawSidePanel()
    {
        GUI.Box(new Rect(0, TopBarHeight, SidebarWidth, Screen.height - TopBarHeight), "");

        var buttonHeight   = 20;
        var buttonSpacing  = 2;
        var count          = (Settings.Mode == ViewerMode.Animations ? AnimationFiles.Count : ModelPaths.Count);
        var listHeight     = count * (buttonHeight + buttonSpacing);
        var sideBarHeight  = Screen.height - TopBarHeight;

        GUILayout.BeginArea(new Rect(0, TopBarHeight, SidebarWidth, sideBarHeight));
        sidePanelScrollPos = GUILayout.BeginScrollView(sidePanelScrollPos, new GUILayoutOption[] { GUILayout.Width(SidebarWidth), GUILayout.Height(sideBarHeight) });
        GUILayout.BeginVertical(new GUILayoutOption[] { GUILayout.Width(SidebarWidth - 20), GUILayout.Height(sideBarHeight) });

        var startIndex    = (int)Math.Floor(sidePanelScrollPos.y / (buttonHeight + buttonSpacing));
        var endIdx        = (int)Math.Min(startIndex + (Screen.height / (buttonHeight + buttonSpacing)), count);
        if (Settings.Mode == ViewerMode.Animations)
        {
            GUILayout.Space(startIndex * (buttonHeight + buttonSpacing));

            for (int i = startIndex; i < endIdx; i++)
            {
                var file = AnimationFiles[i];
                var name = Path.GetFileName(file);
                var isSelected = Settings.CurrentAnimation == file ? "> " : "";
                if (GUILayout.Button($"{isSelected}{name}"))
                {
                    Settings.CurrentAnimation = file;
                    PlayAnimation(file);
                }
            }

            GUILayout.Space((count - endIdx) * (buttonHeight + buttonSpacing));
        }
        else if (Settings.Mode == ViewerMode.Models)
        {
            GUILayout.Space(startIndex * (buttonHeight + buttonSpacing));

            for (int i = startIndex; i < endIdx; i++)
            {
                var file       = ModelPaths[i];
                var name       = Path.GetFileName(file.ModelPath);
                if (int.TryParse(name.Replace(".raw", "").Replace("chr", ""), out int charId) && charId < ChrNames.Length && charId > 0)
                {
                    name = $"{ChrNames[charId - 1]} - {name}";
                }

                var isSelected = Settings.CurrentModel == file.ModelPath ? "> " : "";
                if (GUILayout.Button($"{isSelected}{name}", new GUILayoutOption[] { }))
                {
                    Settings.CurrentModel = file.ModelPath;
                    LoadModel(file.ModelPath);
                }
            }

            GUILayout.Space((count - endIdx) * (buttonHeight + buttonSpacing));
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    // Thanks to RyanUKAus for this list :>
    private string[] ChrNames = new string[]
    {
        "The Colonel",
        "Jungle Queen",
        "Monkey",
        "Stone Golem",
        "Wood Golem",
        "Ramona Sosa",
        "Elijah Jones",
        "Hector Baboso",
        "Jared Slim",
        "Jebidiah Crump",
        "Ample Sally",
        "Lean Molly",
        "Braces",
        "Big Tony",
        "Viola",
        "Stone Golem",
        "Barrel Robot",
        "Railspider Robot",
        "Nikolai",
        "Private Sand",
        "Private Grass",
        "Private Coal",
        "Pirvate Poorly",
        "Sgt Rock",
        "Sgt Shivers",
        "Sgt Wood",
        "Sgt Shock",
        "Sgt Slate",
        "Lt Frost",
        "Lt Wild",
        "Handyman",
        "Lt Shade",
        "Lt Bush",
        "Lt Chill",
        "Wood Golem",
        "Trooper White",
        "Trooper Brown",
        "Trooper Black ",
        "Trooper Green",
        "Trooper Grey",
        "Capt Snow",
        "Capt Sand",
        "Capt Night",
        "Capt Forest",
        "Capt Pain",
        "Hybrid Mutant",
        "Ilsa Nadir",
        "Sentry Bot",
        "Machinist",
        "Chassis Bot",
        "Gretel Mk II",
        "Undead Priest",
        "Louie Bignose",
        "Marco the Snitch",
        "Sewer Zombie",
        "Reaper Splitter",
        "Maiden",
        "Changeling",
        "Portal Daemon",
        "The Master",
        "Krayola",
        "Chastity",
        "Jo-Beth Casey",
        "X-Ray Skel",
        "Leo Krupps",
        "Slick Tommy",
        "Jimmy Needles",
        "Hatchet Sal",
        "Jake Fenton",
        "Lady Jane",
        "NeoTokyo male pedestrian 1",
        "NeoTokyo male pedestrian 2",
        "NeoTokyo male pedestrian 3",
        "NeoTokyo male pedestrian 4",
        "NeoTokyo male pedestrian 5",
        "NeoTokyo male pedestrian 6",
        "NeoTokyo male pedestrian 7",
        "NeoTokyo male pedestrian 8",
        "NeoTokyo male pedestrian 9",
        "NeoTokyo male pedestrian 10",
        "NeoTokyo female pedestrian 1",
        "NeoTokyo female pedestrian 2",
        "NeoTokyo female pedestrian 3",
        "NeoTokyo female pedestrian 4",
        "NeoTokyo female pedestrian 5",
        "NeoTokyo female pedestrian 6",
        "NeoTokyo female pedestrian 7 ",
        "NeoTokyo female pedestrian 8",
        "NeoTokyo female pedestrian 9",
        "Neo Tokyo female pedestrian 10",
        "Accountant",
        "Lawyer",
        "Consultant",
        "Feeder Zombie",
        "Gasmask Special",
        "Cyberfairy",
        "R One-Oh-Seven",
        "Captain Ash",
        "Milkbaby",
        "Sadako",
        "Ghost",
        "Barby Gimp",
        "Riot Officer",
        "R One-Oh-Seven",
        "Jacque de la Morte",
        "The Hunchback",
        "Sgt Cortez",
        "Hank Nova",
        "Kitten Celeste",
        "Bear",
        "Stumpy",
        "Gregor Lenko",
        "Mikey Two-guns",
        "Venus Starr",
        "Harry Tipper",
        "Henchman",
        "Dr. Peabody",
        "Khallos",
        "Aztec Warrior",
        "High Priest",
        "Mister Giggles",
        "Kypriss",
        "Dinosaur",
        "Ozor Mox",
        "Meezor Mox",
        "Candi Skyler",
        "Scourge Splitter",
        "Corp Hart",
        "Drone Splitter",
        "The Cropolite",
        "Female Trooper",
        "Male Trooper",
        "R-109",
        "Mr Underwood",
        "Gargoyle",
        "Crypt Zombie",
        "Lola Varuska",
        "Nikki",
        "Jinki",
        "Ringmistress",
        "Snowman",
        "Cripsin",
        "Baby Drone",
        "Calamari ",
        "Dark Henchman",
        "Sentry Bot",
        "Corp Hart",
        "Sgt Cortez",
        "Sergio",
        "Beetleman",
        "Mischief",
        "The Impersonator",
        "Badass Cyborg",
        "Chinese Chef",
        "Duckman Drake",
        "Gingerbread Man",
        "Insect Mutant",
        "Robofish",
        "Dark Henchman",
        "Dark Henchman",
        "Dark Henchman",
        "Darkhenchman (TimeSplitter Machine in NeoTokyo)",
    };
}
