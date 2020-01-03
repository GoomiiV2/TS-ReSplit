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
using Assets.Scripts.TSFramework.Singletons;

[ExecuteInEditMode]
public class AnimatedModelV2 : MonoBehaviour {
    [HideInInspector] public ModelType ModelType;
    [HideInInspector] public string ModelName;
    public Material DefaultMat;
    public Material DefaultOverlayMat;

    private Vector3 _ModelScale;
    public Vector3 ModelScale { get { return _ModelScale; } }

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

        var tS2Model = ReSplit.Cache.TryCache<TS2.Model>(ModelPath, (path) =>
        {
            var modelFileData = TSAssetManager.LoadFile(ModelPath);
            var model         = new TS2.Model(modelFileData);
            return model;
        }, TSFramework.Singletons.CacheType.ClearOnLevelLoad);

        var isChr = ModelPath.ToUpper().Contains("CHR"); // TODO: use a better way?
        _ModelScale.x      = tS2Model.Scale;

        var modelPakPath = TSAssetManager.GetPakForPath(ModelPath).Item1;
        var texPaths     = TSTextureUtils.GetTexturePathsForMats(tS2Model.Materials);

        if (ModelInfo == null && isChr)
        {
            ModelInfo = new TS2ModelInfo()
            {
                Name     = ModelPath,
                Path     = ModelPath,
                SkelType = TS2AnimationData.SkelationType.Human
            };
        }

        if (ModelInfo != null)
        {
            ModelInfo.BoneToMehses = ModelInfo.BoneToMehses ?? Bonemap.Create(TSAnimationUtils.CreatePartToBoneMap(tS2Model));
        }

        var data = ReSplit.Cache.TryCache<MeshData>($"{ModelPath}_Meshed", (path) =>
        {
            var data2 = TSMeshUtils.SubMeshToMesh(tS2Model.Meshes, new MeshCreationData()
            {
                CreateMainMesh        = true,
                CreateOverlaysMesh    = true,
                CreateTransparentMesh = true,
                IsMapMesh             = false,
                IsSkeletalMesh        = ModelInfo != null && ModelInfo.SkelType != TS2AnimationData.SkelationType.None
            },
            ModelInfo);
            return data2;
        }, TSFramework.Singletons.CacheType.ClearOnLevelLoad);
        

        MeshRenderer.materials = new Material[data.TexData.Length];

        for (int i = 0; i < tS2Model.Materials.Length; i++)
        {
            var matInfo = data.TexData[i];
            var matData = tS2Model.Materials[i];
            Texture2D unityTex;

            if (tS2Model.HasIncludedTextures)
            {
                unityTex = ReSplit.Cache.TryCache($"{ModelPath}_Tex_{i}", (path) =>
                {
                    var tex = TSTextureUtils.TS2TexToT2D(tS2Model.Textures[i]);
                    tex.name = $"{i}";
                    return tex;
                }, TSFramework.Singletons.CacheType.ClearOnLevelLoad);
            }
            else
            {
                var texName = texPaths[i];
                unityTex    = ReSplit.Cache.TryCache($"{modelPakPath}/{texName}", (path) =>
                {
                    var texData        = TSAssetManager.LoadFile(path);
                    var ts2tex         = new TS2.Texture(texData);
                    var tex            = TSTextureUtils.TS2TexToT2D(ts2tex);
                    tex.name           = texName;
                    return tex;
                }, TSFramework.Singletons.CacheType.ClearOnLevelLoad);
            }

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
            if (child.tag != "DontDelete")
            {
                DestroyImmediate(child.gameObject);
            }
        }
        #endif

        if (ModelInfo != null && ModelInfo.SkelType != TS2AnimationData.SkelationType.None)
        {
            var bones               = TSAnimationUtils.CreateModelBindPose(transform.gameObject, ModelInfo, data.Mesh, tS2Model.Scale, tS2Model);
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

        /*if (MeshFilter != null) { DestroyImmediate(MeshFilter); }
        MeshFilter = this.gameObject.AddComponent<MeshFilter>();

        if (MeshRenderer != null) { DestroyImmediate(MeshRenderer); }
        MeshRenderer = this.gameObject.AddComponent<SkinnedMeshRenderer>();*/

        if (MeshFilter == null) { MeshFilter = this.gameObject.AddComponent<MeshFilter>(); }

        if (MeshRenderer == null) { MeshRenderer = this.gameObject.AddComponent<SkinnedMeshRenderer>(); }

        MeshFilter.hideFlags   = HideFlags.DontSave;
        MeshRenderer.hideFlags = HideFlags.DontSave;
    }
}