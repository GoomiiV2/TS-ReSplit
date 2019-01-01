using Assets.Scripts.TSFramework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class TSAnimatedModel : MonoBehaviour {
    public string ModelPath = "ts2/pak/chr.pak/ob/chrs/chr128.raw";
    public Shader Shader;
    public Material DefaultMaterial;

    private TS2.Model TS2Model;

    void Start ()
    {
        var testFile = TSAssetManager.LoadFile(ModelPath);
        Shader = Shader.Find("Custom/BasicChrShader");

        LoadTs2Model(ModelPath);
    }
	
	// Update is called once per frame
	void Update () {

    }

    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
        DrawBones();
        //DisplayVertWeights();
        //DrawVertFlags();
    }

    public void LoadTs2Model(string ModelPath)
    {
        var meshFilter = GetComponent<MeshFilter>();
        var meshRender = GetComponent<SkinnedMeshRenderer>();
        var modelData  = TSAssetManager.LoadFile(ModelPath);
        TS2Model       = new TS2.Model(modelData);

        var modelPakPath = TSAssetManager.GetPakForPath(ModelPath).Item1;
        var texPaths     = TSTextureUtils.GetTexturePathsForMats(TS2Model.Materials);
        var mat          = new Material(Shader);

        var data = TSMeshUtils.SubMeshToMesh(TS2Model.Meshes, new MeshCreationData()
        {
            CreateMainMesh        = true,
            CreateOverlaysMesh    = true,
            CreateTransparentMesh = true,
            IsMapMesh             = false,
            IsSkeletalMesh        = true
        });

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

        //meshRender.materials = null;

        var bones        = CreateSkelation(data.Mesh, TS2Model);
        meshRender.bones = bones;

        meshRender.sharedMesh = data.Mesh;
        meshFilter.mesh       = data.Mesh;
    }

    private Transform[] CreateSkelation(Mesh Mesh, TS2.Model Model)
    {
        var bones    = new List<Transform>();
        var bindPoses = new List<Matrix4x4>();

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

            Transform bone = new GameObject($"Bone {i}").transform;
            bone.parent = transform;
            bone.localRotation = Quaternion.identity;
            bone.localPosition = pos;
            if (meshInfo.Unk3 != 0xFF) { bone.parent = bones[meshInfo.Unk3]; }
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
    private void DrawBones()
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

                    var pos = transform.position + centerPoint;

                    var bone = new TSBone()
                    {
                        Unk1 = meshInfo.Unk1,
                        Unk2 = meshInfo.Unk2,
                        Unk3 = meshInfo.Unk3,
                        Unk4 = meshInfo.ID,
                        Unk5 = meshInfo.Unk4,
                        Unk6 = meshInfo.Unk5,
                        Pos  = pos
                    };

                    bones.Add(bone);

                    Gizmos.DrawSphere(pos, 0.02f);

                    /*var boneLabel   = $"Idx: {i}, {meshInfo.Unk1} - {meshInfo.Unk2} - {meshInfo.Unk3} - {meshInfo.ID}";
                    Handles.Label(pos, boneLabel);*/
                }
                else
                {
                    //Debug.Log($"Mesh {i} had no main mesh, {meshInfo.Unk1} - {meshInfo.Unk2} - {meshInfo.Unk3} - {meshInfo.ID}");

                    var bone = new TSBone()
                    {
                        Unk1 = meshInfo.Unk1,
                        Unk2 = meshInfo.Unk2,
                        Unk3 = meshInfo.Unk3,
                        Unk4 = meshInfo.ID,
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

                if (bone.Unk4 != 0xFF)
                {
                    var connectedTo = bones[bone.Unk4];
                    UnityEngine.Debug.DrawLine(bone.Pos + (Vector3.left * 2), connectedTo.Pos + (Vector3.left * 2), Color.blue);
                }

                if (bone.Unk3 != 0xFF)
                {
                    UnityEngine.Debug.DrawLine(bone.Pos, bones[bone.Unk3].Pos, Color.cyan);
                }

                /*if (bone.Unk2 != 0xFF)
                {
                    Debug.DrawLine(bone.Pos, bones[bone.Unk2].Pos, Color.green);
                }*/file:///C:/Users/Arkii/Downloads/fmt_lithTech_dat.py

                /*if (bone.Unk1 != 0xFF)
                {
                    Debug.DrawLine(bone.Pos, bones[bone.Unk1].Pos, Color.green);
                }*/

                var boneLabel = $"Idx: {i}, {bone.Unk1} - {bone.Unk2} - {bone.Unk3} - {bone.Unk4} - {bone.Unk5} - {bone.Unk6}";
                Handles.Label(bone.Pos, boneLabel,
                    new GUIStyle() {
                        alignment = TextAnchor.MiddleCenter,
                        normal = new GUIStyleState() {
                            textColor = Color.white,
                        },
                        fontSize = 6
                    });

                Gizmos.color = new Color32(224, 51, 94, 255);
                Gizmos.DrawSphere(bone.Pos, 0.002f);
            }
        }

    }

    [Conditional("UNITY_EDITOR")]
    private void DisplayVertWeights()
    {
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
    }

    [Conditional("UNITY_EDITOR")]
    public void DrawVertFlags()
    {
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

                    Handles.Label(pos, $"{vert.Scale}");
                }
            }
        }
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
