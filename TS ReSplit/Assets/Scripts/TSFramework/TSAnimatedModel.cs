using Assets.Scripts.TSFramework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using TS2Data;

[ExecuteInEditMode]
public class TSAnimatedModel : MonoBehaviour {
    public string ModelPath = "ts2/pak/chr.pak/ob/chrs/chr128.raw";
    public Shader Shader;
    public Material DefaultMaterial;

    public AnimationClip TestAnimation;

    private TS2.Model TS2Model;
    private TS2Bone[] Bones;

    const string BONE_PREFIX = "Bone ";

    void Start()
    {
        var testFile = TSAssetManager.LoadFile(ModelPath);
        Shader = Shader.Find("Custom/BasicChrShader");

        LoadTs2Model(ModelPath);
        Bones = TSAnimationUtils.BuildTS2Skelation(TS2Model);

        var playerModelData = ModelDB.Get(ModelType.Player, PlayerModels.Viola);

        Animation anim = GetComponent<Animation>();
        
        var clips = new string[] {
            "ts2/pak/anim.pak/anim/data/ts2/scratchhead.raw",
            //"ts2/pak/anim.pak/anim/data/ts2/peek_m0.raw",
            "ts2/pak/anim.pak/anim/data/ts2/scientist_defusebomb_m0.raw",
            //"ts2/pak/anim.pak/anim/data/ts2/hit_1_m0.raw",
            "ts2/pak/anim.pak/anim/data/ts2/sholdershot2_m0.raw"
        };

        foreach (var clip in clips)
        {
            var animFile = TSAssetManager.LoadFile(clip);
            var animData = new TS2.Animation(animFile);
            var animClip = TSAnimationUtils.ConvertAnimation(animData, playerModelData.Skeleton.Value, clip);

            anim.AddClip(animClip, animClip.name);
            anim.PlayQueued(clip);
        }
    }
	
	// Update is called once per frame
	void Update () {

    }

    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (TS2Model == null)
        {
            Start();
        }
#endif

        DrawMeshConnections();
        //DisplayVertWeights();
        DrawVertFlags();
        //DrawDebugSkelation();
    }

    public void LoadTs2Model(string ModelPath)
    {
        var meshFilter = GetComponent<MeshFilter>();
        var meshRender = GetComponent<SkinnedMeshRenderer>();
        var modelData  = TSAssetManager.LoadFile(ModelPath);
        TS2Model       = new TS2.Model(modelData);

        UnityEngine.Debug.Log($"-- {ModelPath} --");
        UnityEngine.Debug.Log(TS2Model.GetMeshInfosList());

        var boneMap = TSAnimationUtils.CreatePartToBoneMap(TS2Model);

        var modelPakPath = TSAssetManager.GetPakForPath(ModelPath).Item1;
        var texPaths     = TSTextureUtils.GetTexturePathsForMats(TS2Model.Materials);
        var mat          = new Material(Shader);

        var playerModelData = ModelDB.Get(ModelType.Player, PlayerModels.Viola);
        var data = TSMeshUtils.SubMeshToMesh(TS2Model.Meshes, new MeshCreationData()
        {
            CreateMainMesh        = true,
            CreateOverlaysMesh    = true,
            CreateTransparentMesh = true,
            IsMapMesh             = false,
            IsSkeletalMesh        = true
        },
        playerModelData);

        meshRender.materials = new Material[data.TexData.Length];

        for (int i = 0; i < meshRender.materials.Length; i++)
        {
            var matInfo = data.TexData[i];
            TS2.Texture tex;

            if (TS2Model.HasIncludedTextures)
            {
                tex = TS2Model.Textures[i];
            }
            else
            {
                var texName = texPaths[i];
                var texData = TSAssetManager.LoadFile($"{modelPakPath}/{texName}");
                tex         = new TS2.Texture(texData);
            }

            var unityTex = TSTextureUtils.TS2TexToT2D(tex);

            meshRender.materials[i] = DefaultMaterial;
            meshRender.materials[i].CopyPropertiesFromMaterial(DefaultMaterial);
            meshRender.materials[i].mainTexture = unityTex;
        }

        // If running in the editor remove existing bones first
#if UNITY_EDITOR
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            //if (child.name.StartsWith(TSAnimationUtils.BONE_PREFIX))
            {
                DestroyImmediate(child.gameObject);
            }
        }
#endif

        var bones          = TSAnimationUtils.CreateModelBindPose(transform.gameObject, playerModelData, data.Mesh, TS2Model.Scale);
        meshRender.bones   = bones;

        meshRender.hideFlags  = HideFlags.DontSave;
        meshRender.sharedMesh = data.Mesh;
        meshFilter.hideFlags  = HideFlags.DontSave;
        meshFilter.mesh       = data.Mesh;
    }

    private Transform[] CreateSkelation(Mesh Mesh, TS2.Model Model)
    {
        var bones     = new List<Transform>();
        var bindPoses = new List<Matrix4x4>();

        // If running in the editor remove existing bones first
#if UNITY_EDITOR
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith(BONE_PREFIX))
            {
                DestroyImmediate(child.gameObject);
            }
        }
#endif

        for (int i = 0; i < Model.MeshInfos.Length; i++)
        {
            var mesh     = Model.Meshes[i];
            var meshInfo = Model.MeshInfos[i];
            var pos      = transform.position;

            if (mesh.MainMesh != null)
            {
                var points      = mesh.MainMesh.Verts.Select(x => TSMeshUtils.Ts2VertToV3(x)).ToList();
                var centerPoint = Utils.GetCenterOfPoints(points);

                //pos = transform.position + centerPoint;
                pos = centerPoint;
            }

            var debugText = $"{meshInfo.IsBone} - {meshInfo.Unk2} - {meshInfo.ParentIdx} - {meshInfo.ChildIdx} - {meshInfo.Unk4} - {meshInfo.Unk5}";
            Transform bone     = new GameObject($"{BONE_PREFIX}{i} ({debugText})") { hideFlags = HideFlags.DontSave }.transform;
            bone.parent        = transform;
            bone.localRotation = Quaternion.identity;
            bone.localPosition = pos;
            if (meshInfo.ParentIdx != 0xFF) { bone.parent = bones[meshInfo.ParentIdx]; }
            bones.Add(bone);

            Matrix4x4 bindPose = bone.worldToLocalMatrix * transform.localToWorldMatrix;
            bindPoses.Add(bindPose);
        }

        Mesh.bindposes = bindPoses.ToArray();

        return bones.ToArray();
    }

    void CreateFromTS2Model(TS2.Model Model)
    {
        var mesh = TSMeshUtils.TS2ModelToMesh(Model);

        GetComponent<MeshFilter>().mesh = mesh;
    }

    [Conditional("UNITY_EDITOR")]
    private void DrawMeshConnections()
    {
        if (TS2Model != null)
        {
            Gizmos.color = new Color32(224, 51, 94, 255);
            var bones = new List<TSBone>();

            for (int i = 0; i < TS2Model.Meshes.Length; i++)
            {
                var mesh     = TS2Model.Meshes[i];
                var meshInfo = TS2Model.MeshInfos[i];

                if (mesh.MainMesh != null)
                {
                    var points = mesh.MainMesh.Verts.Select(x => TSMeshUtils.Ts2VertToV3(x)).ToList();
                    var centerPoint = Utils.GetCenterOfPoints(points);

                    if (points.Count == 0)
                    {
                        centerPoint = bones[meshInfo.ParentIdx].Pos;
                    }

                    var pos = transform.position + centerPoint;

                    var bone = new TSBone()
                    {
                        Unk1 = meshInfo.IsBone,
                        Unk2 = meshInfo.Unk2,
                        Unk3 = meshInfo.ParentIdx,
                        Unk4 = meshInfo.ChildIdx,
                        Unk5 = meshInfo.Unk4,
                        Unk6 = meshInfo.Unk5,
                        Pos  = pos
                    };

                    bones.Add(bone);

#if UNITY_EDITOR
                    //var boneLabel   = $"Idx: {i}, {meshInfo.ChildIdx} - {meshInfo.Unk4} - {meshInfo.Unk5} - {meshInfo.IsBone}";
                    var boneLabel = $"Idx: {i}, {meshInfo.Unk2} - {meshInfo.IsBone}";
                    Handles.Label(pos, boneLabel);
#endif
                }
                else
                {
                    //Debug.Log($"Mesh {i} had no main mesh, {meshInfo.Unk1} - {meshInfo.Unk2} - {meshInfo.Unk3} - {meshInfo.ID}");

                    var bone = new TSBone()
                    {
                        Unk1 = meshInfo.IsBone,
                        Unk2 = meshInfo.Unk2,
                        Unk3 = meshInfo.ParentIdx,
                        Unk4 = meshInfo.ChildIdx,
                        Unk5 = meshInfo.Unk4,
                        Unk6 = meshInfo.Unk5,
                        Pos  = transform.position
                    };

                    bones.Add(bone);
                }
            }

            for (int i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];

                /*if (bone.Unk4 != 0xFF)
                {
                    var connectedTo = bones[bone.Unk4];
                    UnityEngine.Debug.DrawLine(bone.Pos, connectedTo.Pos, Color.blue);
                }*/

                if (bone.Unk3 != 0xFF)
                {
                    //UnityEngine.Debug.DrawLine(bone.Pos, bones[bone.Unk3].Pos, Color.cyan);
                }

                /*if (bone.Unk2 != 0xFF)
                {
                    Debug.DrawLine(bone.Pos, bones[bone.Unk2].Pos, Color.green);
                }*/

                /*if (bone.Unk1 != 0xFF)
                {
                    Debug.DrawLine(bone.Pos, bones[bone.Unk1].Pos, Color.green);
                }*/

                //var boneLabel = $"Idx: {i}, {bone.Unk1} - {bone.Unk2} - {bone.Unk3} - {bone.Unk4} - {bone.Unk5} - {bone.Unk6}";
                var boneLabel = $"Idx: {i}";
                //UNITY_EDITOR
                #if FALSE
                    Handles.Label(bone.Pos + (Vector3.up * 0.00002f), boneLabel,
                        new GUIStyle() {
                            alignment = TextAnchor.MiddleCenter,
                            normal = new GUIStyleState() {
                                textColor = Color.white,
                            },
                            fontSize = 10
                        });

                    Gizmos.color = new Color32(224, 51, 94, 150);
                    Gizmos.DrawWireSphere(bone.Pos, 0.02f);
                #endif
            }
        }

    }

    [Conditional("UNITY_EDITOR")]
    private void DisplayVertWeights()
    {
        #if UNITY_EDITOR
            for (int i = 0; i < TS2Model.Meshes.Length; i++)
            {
                var mesh = TS2Model.Meshes[i];
                if (mesh.MainMesh != null)
                {
                    for (int aye = 0; aye < mesh.MainMesh.Verts.Length; aye++)
                    {
                        var vert = mesh.MainMesh.Verts[aye];
                        var weight = mesh.MainMesh.Uvs[aye];

                        var pos = transform.position + TSMeshUtils.Ts2VertToV3(vert);

                        if (weight.W != 1.0f)
                        {
                            Handles.Label(pos, $"{weight.W}");
                        }
                    }
                }
            }
        #endif
    }

    [Conditional("UNITY_EDITOR")]
    public void DrawVertFlags()
    {
        #if UNITY_EDITOR
            for (int i = 0; i < TS2Model.Meshes.Length; i++)
            {
                var mesh = TS2Model.Meshes[i];
                if (mesh.MainMesh != null)
                {
                    for (int aye = 0; aye < mesh.MainMesh.Verts.Length; aye++)
                    {
                        var vert    = mesh.MainMesh.Verts[aye];
                        var weight  = mesh.MainMesh.Uvs[aye];
                        var normal = mesh.MainMesh.Normals[aye];

                        var pos = transform.position + TSMeshUtils.Ts2VertToV3(vert);

                        if (normal.SameStrip != 0)
                        {
                            Handles.Label(pos, $"{normal.SameStrip}");
                        }
                    }
                }
            }
        #endif
    }

    [Conditional("UNITY_EDITOR")]
    private void DrawDebugSkelation()
    {
        #if UNITY_EDITOR
            if (Bones != null)
            {
                for (int i = 0; i < Bones.Length; i++)
                {
                    var bone     = Bones[i];
                    var pos      = transform.position + bone.Position;
                    Gizmos.color = Color.yellow;
                    //Gizmos.DrawSphere(pos, 0.02f);
                    Handles.Label(pos, $"{bone.ID} [{string.Join(", ", bone.MeshSections.ToArray())}]");
                }
            }
        #endif
    }
}

public struct TSBone
{
    public byte Unk1;
    public byte Unk2;
    public byte Unk3;
    public byte Unk4;
    public byte Unk5;
    public byte Unk6;
    public Vector3 Pos;
}