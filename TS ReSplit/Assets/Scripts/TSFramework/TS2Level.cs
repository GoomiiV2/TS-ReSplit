using Assets.Scripts.TSFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TS2;
using UnityEditor;
using UnityEngine;

public class TS2Level : MonoBehaviour
{
    public string LevelPak = "ts2/pak/story/l_35_ST.pak";
    public string LevelID  = "35";
    public Shader Shader;
    public bool ShowVisLeafs = false;

    private string LevelDataPath { get { return $"{LevelPak}/bg/level{LevelID}/level{LevelID}.raw"; } }
    private Texture2D[] Textures;

    // Game objects to parent differnt entitys under
    private GameObject LevelBase   = null;
    private GameObject SectionBase = null;
    private GameObject DebugBase   = null;

    // Start is called before the first frame update
    void Start()
    {
        // Create a root gameobject to parent level objects under
        LevelBase   = new GameObject("Level Base");
        SectionBase = new GameObject("Sections");
        DebugBase   = new GameObject("Debug");

        SectionBase.transform.SetParent(LevelBase.transform);
        DebugBase.transform.SetParent(LevelBase.transform);

        Load();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Load()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var meshRender = GetComponent<MeshRenderer>();
        var mapData    = TSAssetManager.LoadFile(LevelDataPath);

        var level = new TS2.Map(mapData);

        LoadTextures(level.Materials);

        for (int i = 0; i < level.Sections.Count; i++)
        {
            var section = level.Sections[i];
            CreateSectionGameObj(section);
        }

        if (ShowVisLeafs) { CreateVisLeafs(level); }
    }

    private void LoadTextures(TS2.MatInfo[] MaterialInfos)
    {
        var modelPakPath = TSAssetManager.GetPakForPath(LevelDataPath).Item1;
        var texPaths     = TSTextureUtils.GetTexturePathsForMats(MaterialInfos);
        var mat          = new Material(Shader);

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

        var mat    = new Material(Shader);

        meshRender.materials = new Material[Textures.Length];

        for (int i = 0; i < Textures.Length; i++)
        {
            meshRender.materials[i]             = mat;
            meshRender.materials[i].mainTexture = Textures[i];
            meshRender.materials[i].shader      = Shader;
        }

        meshFilter.mesh              = TSMeshUtils.TS2MeshToMesh(Section.Mesh, Textures.Length);
        MeshUtility.Optimize(meshFilter.mesh);
        meshRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

        sectionGObj.transform.SetParent(SectionBase.transform);
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

    private void CreateDebugPoint(Vector3 Pos, Color? PointColor, string Label = "")
    {
        var point                  = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        point.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        point.transform.position   = Pos;
        point.transform.SetParent(DebugBase.transform);
    }
}
