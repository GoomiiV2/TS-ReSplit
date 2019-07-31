using Assets.Scripts.TSFramework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using TS2Data;
using TSData;

[ExecuteInEditMode]
public class AnimatedModelV2 : MonoBehaviour {
    [HideInInspector] public ModelType ModelType;
    [HideInInspector] public string ModelName;
    public Material DefaultMat;
    public Material DefaultOverlayMat;

    private MeshFilter MeshFilter;
    private SkinnedMeshRenderer MeshRenderer;

    void Start()
    {
        hideFlags = HideFlags.None;
        LoadModel();
    }

    public void LoadModel()
    {
        var modelData = ModelDB.Get(ModelType, ModelName);
        if (modelData == null) { return; }

        LoadModel(modelData.Path, modelData);
    }

    public void LoadModel(string ModelPath, TS2ModelInfo ModelInfo)
    {
        var sw           = Stopwatch.StartNew();
        var orgRotation = transform.rotation;

        transform.rotation = Quaternion.identity;

        CreateComponets();

        var modelFileData  = TSAssetManager.LoadFile(ModelPath);
        var tS2Model       = new TS2.Model(modelFileData);

        var modelPakPath = TSAssetManager.GetPakForPath(ModelPath).Item1;
        var texPaths     = TSTextureUtils.GetTexturePathsForMats(tS2Model.Materials);
        var data         = TSMeshUtils.SubMeshToMesh(tS2Model.Meshes, new MeshCreationData()
        {
            CreateMainMesh        = true,
            CreateOverlaysMesh    = true,
            CreateTransparentMesh = true,
            IsMapMesh             = false,
            IsSkeletalMesh        = ModelInfo != null && ModelInfo.SkelType != TS2AnimationData.SkelationType.None
        },
        ModelInfo);

        MeshRenderer.materials = new Material[data.TexData.Length];

        for (int i = 0; i < MeshRenderer.materials.Length; i++)
        {
            var matInfo = data.TexData[i];
            var matData = tS2Model.Materials[i];
            TS2.Texture tex;

            if (tS2Model.HasIncludedTextures)
            {
                tex = tS2Model.Textures[i];
            }
            else
            {
                var texName = texPaths[i];
                var texData = TSAssetManager.LoadFile($"{modelPakPath}/{texName}");
                tex         = new TS2.Texture(texData);
            }

            var unityTex                = TSTextureUtils.TS2TexToT2D(tex);

            /* if (matInfo.IsTransparent)
            {
                MeshRenderer.materials[i]             = new Material(DefaultOverlayMat);
                MeshRenderer.materials[i].CopyPropertiesFromMaterial(DefaultOverlayMat);
                MeshRenderer.materials[i].mainTexture = unityTex;

                MeshRenderer.materials[i].SetFloat("_ClampUVsX", matData.WrapModeX == TS2.MatInfo.WrapMode.NoRepeat ? 1 : 0);
                MeshRenderer.materials[i].SetFloat("_ClampUVsY", matData.WrapModeY == TS2.MatInfo.WrapMode.NoRepeat ? 1 : 0);
            }
            else*/
            {
                MeshRenderer.materials[i]   = DefaultMat;
                MeshRenderer.materials[i].CopyPropertiesFromMaterial(DefaultMat);
                MeshRenderer.materials[i].mainTexture = unityTex;
            }
        }

        // If running in the editor remove existing bones first
        #if UNITY_EDITOR
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            DestroyImmediate(child.gameObject);
        }
        #endif

        if (ModelInfo != null && ModelInfo.SkelType != TS2AnimationData.SkelationType.None)
        {
            var bones               = TSAnimationUtils.CreateModelBindPose(transform.gameObject, ModelInfo, data.Mesh, tS2Model.Scale);
            MeshRenderer.bones      = bones;
            //MeshRenderer.rootBone   = bones[0];
        }

        MeshRenderer.sharedMesh  = data.Mesh;
        MeshRenderer.localBounds = data.Mesh.bounds;
        MeshFilter.mesh          = data.Mesh;

        sw.Stop();
        UnityEngine.Debug.Log($"Loading model {ModelPath} took {sw.Elapsed.ToString("ss's - 'fff'ms'")}");
        

        /*var avatar = TSAnimationUtils.CreateAvatar(this.gameObject);
        AssetDatabase.CreateAsset(avatar, "Assets/test/HumanAvatarTest.asset");

        var animFile = TSAssetManager.LoadFile("ts2/pak/anim.pak/anim/data/ts2/scratchhead.raw");
        var animData = new TS2.Animation(animFile);
        var animClip = TSAnimationUtils.ConvertAnimation(animData, modelDBData.Skeleton.Value, "scratchhead");
        AssetDatabase.CreateAsset(animClip, "Assets/test/HeadScratchAnim.asset");*/

        transform.rotation = orgRotation;
    }

    private void CreateComponets()
    {
        MeshFilter   = GetComponent<MeshFilter>();
        MeshRenderer = GetComponent<SkinnedMeshRenderer>();

        if (MeshFilter == null)
        {
            MeshFilter = this.gameObject.AddComponent<MeshFilter>();
        }

        if (MeshRenderer == null)
        {
            MeshRenderer = this.gameObject.AddComponent<SkinnedMeshRenderer>();
        }

        MeshFilter.hideFlags   = HideFlags.DontSave;
        MeshRenderer.hideFlags = HideFlags.DontSave;
    }
}