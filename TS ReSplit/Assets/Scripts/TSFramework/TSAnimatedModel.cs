using Assets.Scripts.TSFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TSAnimatedModel : MonoBehaviour {
    public string ModelPath = "ts2/pak/chr.pak/ob/chrs/chr128.raw";
    public Shader Shader;

	void Start ()
    {
        var testFile = TSAssetManager.LoadFile(ModelPath);
        Shader = Shader.Find("Custom/BasicChrShader");

        LoadTs2Model(ModelPath);
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void LoadTs2Model(string ModelPath)
    {
        var meshFilter = GetComponent<MeshFilter>();
        var meshRender = GetComponent<MeshRenderer>();
        var modelData  = TSAssetManager.LoadFile(ModelPath);
        var model      = new TS2.Model(modelData);

        var modelPakPath = TSAssetManager.GetPakForPath(ModelPath).Item1;
        var texPaths     = TSTextureUtils.GetTexturePathsForMats(model.Materials);
        var mat          = new Material(Shader);

        meshRender.materials = new Material[texPaths.Length];

        for (int i = 0; i < texPaths.Length; i++)
        {
            var texPath = texPaths[i];
            var texData = TSAssetManager.LoadFile($"{modelPakPath}/{texPath}");
            var ts2tex  = new TS2.Texture(texData);
            var tex     = TSTextureUtils.TS2TexToT2D(ts2tex);

            meshRender.materials[i]             = mat;
            meshRender.materials[i].mainTexture = tex;
        }

        var mesh        = TSMeshUtils.TS2ModelToMesh(model);
        meshFilter.mesh = mesh;
    }

    void CreateFromTS2Model(TS2.Model Model)
    {
        var mesh = TSMeshUtils.TS2ModelToMesh(Model);

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
