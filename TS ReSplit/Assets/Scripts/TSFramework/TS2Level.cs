using Assets.Scripts.TSFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TS2;
using UnityEngine;

public class TS2Level : MonoBehaviour
{
    public string LevelPak = "ts2/pak/story/l_35_ST.pak";
    public string LevelID = "35";
    public Shader Shader;

    private string LevelDataPath { get { return $"{LevelPak}/bg/level{LevelID}/level{LevelID}.raw"; } }
    private Texture2D[] Textures;

    // Start is called before the first frame update
    void Start()
    {
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

        #region test
        /*var mesh = new Mesh();
        mesh.SetVertices(level.VertsTemp.Select(x => TSMeshUtils.Ts2VertToV3(x)).ToList());
        //mesh.SetNormals(level.NormsTemp.Select(x => TSMeshUtils.Ts2VertToV3(x)).ToList());

        List<int> indices = new List<int>();
        int currentIdx    = 0;
        bool faceDir      = false;
        for (int eye      = 0; eye < level.VertsTemp.Count; eye++)
        {
            var vert = level.VertsTemp[eye];

            var indiceOffset = currentIdx;
            currentIdx++;

            if (vert.Flag == 0)
            {
                if ((faceDir && vert.SameStrip == 1) || (!faceDir && vert.SameStrip == 0))
                {
                    indices.AddRange(new int[] {
                                indiceOffset - 2,
                                indiceOffset - 1,
                                indiceOffset });
                }
                else
                {
                    indices.AddRange(new int[] {
                                indiceOffset - 1,
                                indiceOffset - 2,
                                indiceOffset });
                }
            }
            else
            {
                faceDir = true;
            }

            faceDir = !faceDir;
        }

        mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        mesh.UploadMeshData(true);

        meshFilter.mesh = mesh;*/
        #endregion

        LoadTextures(level.Materials);

        for (int i = 0; i < level.Sections.Count; i++)
        {
            var section = level.Sections[i];
            CreateSectionGameObj(section);
        }
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
        sectionGObj.transform.localScale = new Vector3(-1, 1, 1);

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
        meshRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    }
}
