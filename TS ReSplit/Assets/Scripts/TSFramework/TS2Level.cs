using Assets.Scripts.TSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TS2;
using UnityEditor;
using UnityEngine;

using TextureStore = System.Collections.Generic.Dictionary<uint, (UnityEngine.Texture2D Tex, TS2.Texture.MetaInfo Meta)>;

[ExecuteInEditMode]
public class TS2Level : MonoBehaviour
{
    public string LevelPak = "ts2/pak/story/l_35_ST.pak";
    public string LevelID  = "35";
    public Refrances DataAssests;
    public bool ShowVisLeafs        = false;
    public bool CreateCollision     = false;
    public bool ImportMainGeo       = true;
    public bool ImportDecalGeo      = true;
    public bool ImportOverlayGeo    = true;
    public LevelBakedData BakedData = new LevelBakedData();

    private string LevelDataPath { get { return $"{LevelPak}/bg/level{LevelID}/level{LevelID}.raw"; } }
    private string LevelPadPath { get { return $"{LevelPak}/pad/data/level{LevelID}.raw"; } }
    private TextureStore Textures;
    private TS2.Pathing LevelPad;

    // Some debuging stuff
    private Stopwatch PrefTimer = new Stopwatch();
    private TimeSpan LastPrefTime;

    // Game objects to parent differnt entitys under
    [System.NonSerialized] public GameObject LevelBase   = null;
    [System.NonSerialized] public GameObject SectionBase = null;
    [System.NonSerialized] public GameObject DebugBase   = null;
    [System.NonSerialized] public GameObject PathingBase = null;

    // Start is called before the first frame update
    public void Start()
    {
        TidyUpIfEditor();

        // Create a root gameobject to parent level objects under
        LevelBase   = new GameObject("Level Base")  { isStatic = true };
        SectionBase = new GameObject("Sections")    { isStatic = true };
        DebugBase   = new GameObject("Debug")       { isStatic = true };
        PathingBase = new GameObject("Pathing")     { isStatic = true };

        SectionBase.transform.SetParent(LevelBase.transform);
        DebugBase.transform.SetParent(LevelBase.transform);
        PathingBase.transform.SetParent(LevelBase.transform);

        Load();

        // Flip to look correct
        LevelBase.transform.localScale = new Vector3(-1, 1, 1);

        // Swap back to the scene view, nicer for dev :>
        #if UNITY_EDITOR
        //UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Load()
    {
        PrefLog(LoadingStage.Starting);

        var meshFilter = GetComponent<MeshFilter>();
        var meshRender = GetComponent<MeshRenderer>();
        var mapData    = TSAssetManager.LoadFile(LevelDataPath);
        var padData    = TSAssetManager.LoadFile(LevelPadPath);
        PrefLog(LoadingStage.LevelDataLoad);

        var level = new TS2.Map(mapData);
        if (padData != null)
        {
            LevelPad = new TS2.Pathing(padData);
        }
        PrefLog(LoadingStage.LevelDataParsing);

        Textures = new TextureStore();
        LoadTextures(level.Materials);
        PrefLog(LoadingStage.TextureLoadingCreation);

        for (int i = 0; i < level.Sections.Count; i++)
        {
            var section = level.Sections[i];
            CreateSectionGameObj(section, level, i);
        }
        PrefLog(LoadingStage.SectionCreation);

        if (ShowVisLeafs) { CreateVisLeafs(level); }

        CreatePathingNodes();
        CreatePortalDoors(level);
        PrefLog(LoadingStage.Misc);

        /*for (int i = 0; i < level.PossablePositions.Count; i++)
        {
            var posAndOffset = level.PossablePositions[i];
            var pos = new Vector3(posAndOffset.Item2[0], posAndOffset.Item2[1], posAndOffset.Item2[2]);

            CreateDebugPoint(pos, null, $"{posAndOffset.Item1}");
        }*/

        /*for (int i = 0; i < level.Positions.Count; i++)
        {
            var posAndRotation = level.Positions[i];
            var pos            = new Vector3(posAndRotation[0], posAndRotation[1], posAndRotation[2]);
            var rot            = new Vector3(posAndRotation[3], posAndRotation[4], posAndRotation[5]);

            CreateDebugPoint(pos);
        }*/

        PrefLog(LoadingStage.Total);
    }

    // If we playied from the editor tidy up somethings first
    private void TidyUpIfEditor()
    {
        if (Application.isEditor)
        {
            UnityEngine.Debug.Log("Running inside the editor, tidying up");
            ClearGenratedContent();
        }
    }

    private void LoadTextures(TS2.MatInfo[] MaterialInfos)
    {
        var modelPakPath = TSAssetManager.GetPakForPath(LevelDataPath).Item1;
        var texPaths     = TSTextureUtils.GetTexturePathsForMats(MaterialInfos);
        var mat          = new Material(DataAssests.DefaultShader);

        for (int i = 0; i < texPaths.Length; i++)
        {
            var texID              = MaterialInfos[i];
            var isTexAlreadyLoaded = Textures.ContainsKey(texID.ID);

            if (!isTexAlreadyLoaded)
            {
                var texPath = texPaths[i];
                var texData = TSAssetManager.LoadFile($"{modelPakPath}/{texPath}");
                var ts2tex  = new TS2.Texture(texData);
                var tex     = TSTextureUtils.TS2TexToT2D(ts2tex);

                Textures.Add(texID.ID, (tex, ts2tex.Meta));
            }
        }
    }

    // Gets a texture from the cache, or loads it if it was missing
    private (Texture2D Tex, TS2.Texture.MetaInfo Meta) GetTexture(uint MatInfoId)
    {
        var isLoaded = Textures.TryGetValue(MatInfoId, out (Texture2D Tex, TS2.Texture.MetaInfo Meta) Tex);
        if (isLoaded)
        {
            return Tex;
        }
        else
        {
            var texArr = new TS2.MatInfo[] { new MatInfo() { ID = MatInfoId } };
            LoadTextures(texArr);

            return Textures[MatInfoId];
        }
    }

    private void CreateSectionGameObj(Section Section, TS2.Map Map, int Idx)
    {
        UnityEngine.Debug.Log($"Creating section: {Section.ID}");

        var meshGenOpts = new MeshCreationData()
        {
            CreateMainMesh        = ImportMainGeo,
            CreateOverlaysMesh    = ImportOverlayGeo,
            CreateTransparentMesh = ImportDecalGeo,
            IsMapMesh             = true
        };

        var mesh                         = TSMeshUtils.SubMeshToMesh(Section.Mesh, meshGenOpts);
        var sectionGObj                  = new GameObject($"Map Section {Idx}");
        sectionGObj.transform.localScale = new Vector3(1, 1, 1);

        var meshRender   = sectionGObj.AddComponent<MeshRenderer>();
        var meshFilter   = sectionGObj.AddComponent<MeshFilter>();
        var collider     = sectionGObj.AddComponent<MeshCollider>();

        var mat                    = new Material(DataAssests.DefaultShader);
        var mats                   = mesh.TexData;
        meshRender.materials       = new Material[mats.Length];

        for (int i = 0; i < meshRender.materials.Length; i++)
        {
            var levelMat                        = Map.Materials[mats[i].TexId];
            var matId                           = levelMat.ID;
            var hasTex                          = Textures.TryGetValue(matId, out var tex);

            if (!hasTex) { tex = (DataAssests.MissingTexture, new TS2.Texture.MetaInfo() { HasAlpha = false }); } // Incase a texture is missing, for some reason

            if (mats[i].IsTransparent)
            {
                meshRender.materials[i]             = new Material(DataAssests.DefaultShader);
                meshRender.materials[i].mainTexture = tex.Tex;
                meshRender.materials[i].shader      = DataAssests.TransparentShader;

                meshRender.materials[i].SetFloat("_ClampUVsX", levelMat.WrapModeX == MatInfo.WrapMode.NoRepeat ? 1 : 0);
                meshRender.materials[i].SetFloat("_ClampUVsY", levelMat.WrapModeY == MatInfo.WrapMode.NoRepeat ? 1 : 0);
            }
            else
            {

                meshRender.materials[i] = new Material(DataAssests.DefaultShader);
                meshRender.materials[i].CopyPropertiesFromMaterial(DataAssests.DefaultMat);
                meshRender.materials[i].mainTexture = tex.Tex;
            }
        }

        meshFilter.mesh = mesh.Mesh;
        meshRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

        if (CreateCollision)
        {
            collider.sharedMesh = meshFilter.mesh;
            //collider.cookingOptions = MeshColliderCookingOptions.EnableMeshCleaning & MeshColliderCookingOptions.WeldColocatedVertices;
        }

        sectionGObj.transform.SetParent(SectionBase.transform);
        sectionGObj.isStatic  = true;
        sectionGObj.tag       = "LevelSection";

        // Apply per section overrides
        if (BakedData != null && BakedData.PerSectionData != null && BakedData.PerSectionData.Count > Idx)
        {
            var sectionData = BakedData.PerSectionData[Idx];
            sectionData.Apply(sectionGObj);
        }
    }

    private void CreatePathingNodes()
    {
        if (LevelPad == null) { return; }

        var nodesLookup = new Dictionary<uint, GameObject>();

        // Create the nodes
        for (int i = 0; i < LevelPad.PathingNodes.Length; i++)
        {
            var node        = LevelPad.PathingNodes[i];
            var nodeGO      = Instantiate(DataAssests.PathingNode);
            var pathingNode = nodeGO.GetComponent<PathingNode>();
            pathingNode.SetNodeData(node);
            nodeGO.transform.SetParent(PathingBase.transform);

            nodesLookup.Add(node.ID, nodeGO);
        }

        // Now link them
        for (int i = 0; i < LevelPad.PathingLinks.Length; i++)
        {
            var link   = LevelPad.PathingLinks[i];
            var parent = nodesLookup[link.ParentNodeID];
            var child  = nodesLookup[link.ChildNodeID];

            var parentPNode = parent.GetComponent<PathingNode>();
            parentPNode.LinkedNodes.Add(child);
        }

        // Add the extra nodes
        for (int i = 0; i < LevelPad.ExtraNodes.Length; i++)
        {
            var node = LevelPad.ExtraNodes[i];

            var nodeGO      = Instantiate(DataAssests.PathingNode);
            var pathingNode = nodeGO.GetComponent<PathingNode>();
            pathingNode.SetNodeData(node, true);

            var linkedNode = nodesLookup[node.UNK2];
            pathingNode.LinkedNodes.Add(linkedNode);

            nodeGO.transform.SetParent(PathingBase.transform);
        }
    }

    private void CreateVisLeafs(TS2.Map Level)
    {
        // Testing, drawing some points
        for (int i = 0; i < Level.VisPortals.Count; i++)
        {
            var visPortal = Level.VisPortals[i];

            // Messy and create garbage? yes, is just for debuging yes :>
            //var points = Level.Boxes[i].Select((x, eye) => new { Index = eye, Value = x }).GroupBy(x => x.Index / 3).Select(x => x.Select(v => v.Value).ToList()).Select(x => new Vector3(x[0], x[1], x[2])).ToList();

            var points = visPortal.Points.Select(x => new Vector3(x[0], x[1], x[2])).ToList();
            var plane  = DebugPlane.CreateFromPoints(points);
            plane.transform.SetParent(DebugBase.transform);
        }
    }

    private void CreatePortalDoors(TS2.Map Level)
    {
        if (Level.PortalDoors != null)
        {
            for (int i = 0; i < Level.PortalDoors.Count; i++)
            {
                var entityDef  = Level.PortalDoors[i];
                var pos        = Utils.V3FromFloats(entityDef.Position);
                var dims       = Utils.V3FromFloats(entityDef.Dimensions);

                var dbgObj = DebugDoor.Create(pos, dims, "Door?", entityDef);
                dbgObj.transform.SetParent(DebugBase.transform);
            }
        }
    }

    private void CreateDebugPoint(Vector3 Pos, Color? PointColor = null, string Label = "")
    {
        var point                  = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Renderer rend              = point.GetComponent<Renderer>();

        point.name                 = Label;
        rend.material.color        = PointColor.HasValue ? PointColor.Value : Color.white;
        point.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        point.transform.position   = Pos;
        point.transform.SetParent(DebugBase.transform);
    }

    // Debug log some load times
    private void PrefLog(LoadingStage Stage)
    {
        var timeTaken = PrefTimer.Elapsed;

        if (Stage == LoadingStage.Starting)
        {
            PrefTimer    = Stopwatch.StartNew();
            LastPrefTime = timeTaken;
            UnityEngine.Debug.Log("Starting level loading pref timer");
        }
        else if (Stage == LoadingStage.Total)
        {
            PrefTimer.Stop();
            var totalElapsed = PrefTimer.Elapsed;
            UnityEngine.Debug.Log($"Level load took: {totalElapsed.ToString("ss's - 'fff'ms'")}");
        }
        else
        {
            var delta = timeTaken - LastPrefTime;
            UnityEngine.Debug.Log($"Level load: {Stage.ToString()} took {delta.ToString("ss's - 'fff'ms'")}");
        }

        LastPrefTime = timeTaken;
    }

    public void ClearGenratedContent()
    {
        const string GEN_BASE_OBJ_NAME = "Level Base";

        var levelBase = GameObject.Find(GEN_BASE_OBJ_NAME);

        if (levelBase != null)
        {
            DestroyImmediate(levelBase);
            ClearGenratedContent(); // Recursive incase their are multiple
        }
    }

    private enum LoadingStage
    {
        Starting,
        LevelDataLoad,
        LevelDataParsing,
        TextureLoadingCreation,
        SectionCreation,
        Misc,
        Total
    }

    [System.Serializable]
    public struct Refrances
    {
        public GameObject PathingNode;
        public Shader DefaultShader;
        public Shader TransparentShader;
        public Texture2D MissingTexture;

        public Material DefaultMat;
    }
}
