using Assets.Scripts.TSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TS2;
using UnityEditor;
using UnityEngine;

public class TS2Level : MonoBehaviour
{
    public string LevelPak = "ts2/pak/story/l_35_ST.pak";
    public string LevelID  = "35";
    public Refrances DataAssests;
    public bool ShowVisLeafs = false;

    private string LevelDataPath { get { return $"{LevelPak}/bg/level{LevelID}/level{LevelID}.raw"; } }
    private string LevelPadPath { get { return $"{LevelPak}/pad/data/level{LevelID}.raw"; } }
    private Texture2D[] Textures;
    private TS2.Pathing LevelPad;

    // Some debuging stuff
    private Stopwatch PrefTimer = new Stopwatch();
    private TimeSpan LastPrefTime;

    // Game objects to parent differnt entitys under
    private GameObject LevelBase   = null;
    private GameObject SectionBase = null;
    private GameObject DebugBase   = null;
    private GameObject PathingBase = null;

    // Start is called before the first frame update
    void Start()
    {
        // Create a root gameobject to parent level objects under
        LevelBase   = new GameObject("Level Base");
        SectionBase = new GameObject("Sections");
        DebugBase   = new GameObject("Debug");
        PathingBase = new GameObject("Pathing");

        SectionBase.transform.SetParent(LevelBase.transform);
        DebugBase.transform.SetParent(LevelBase.transform);
        PathingBase.transform.SetParent(LevelBase.transform);

        Load();

        // Flip to look correct
        LevelBase.transform.localScale = new Vector3(-1, 1, 1);
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
        LevelPad  = new TS2.Pathing(padData);
        PrefLog(LoadingStage.LevelDataParsing);

        LoadTextures(level.Materials);
        PrefLog(LoadingStage.TextureLoadingCreation);

        for (int i = 0; i < level.Sections.Count; i++)
        {
            var section = level.Sections[i];
            CreateSectionGameObj(section);
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

    private void LoadTextures(TS2.MatInfo[] MaterialInfos)
    {
        var modelPakPath = TSAssetManager.GetPakForPath(LevelDataPath).Item1;
        var texPaths     = TSTextureUtils.GetTexturePathsForMats(MaterialInfos);
        var mat          = new Material(DataAssests.DefaultShader);

        Textures = new Texture2D[texPaths.Length];

        for (int i = 0; i < texPaths.Length; i++)
        {
            var texPath = texPaths[i];

            var texData = TSAssetManager.LoadFile($"{modelPakPath}/{texPath}");
            var ts2tex  = new TS2.Texture(texData);
            var tex     = TSTextureUtils.TS2TexToT2D(ts2tex);

            Textures[i] = tex;
        }
    }

    private void CreateSectionGameObj(Section Section)
    {
        var sectionGObj  = new GameObject("Map Section");
        sectionGObj.transform.localScale = new Vector3(1, 1, 1);

        var meshRender   = sectionGObj.AddComponent<MeshRenderer>();
        var meshFilter   = sectionGObj.AddComponent<MeshFilter>();

        var mat    = new Material(DataAssests.DefaultShader);

        meshRender.materials = new Material[Textures.Length];

        for (int i = 0; i < Textures.Length; i++)
        {
            meshRender.materials[i]             = mat;
            meshRender.materials[i].mainTexture = Textures[i];
            meshRender.materials[i].shader      = DataAssests.DefaultShader;
        }

        meshFilter.mesh              = TSMeshUtils.TS2MeshToMesh(Section.Mesh, Textures.Length);
        MeshUtility.Optimize(meshFilter.mesh);
        meshRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

        sectionGObj.transform.SetParent(SectionBase.transform);
    }

    private void CreatePathingNodes()
    {
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
    }
}
