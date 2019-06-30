using Assets.Scripts.TSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TSData;
using UnityEngine;

// Helpers for dealing with animations and converting them to a Unity happy format
public static class TSAnimationUtils
{
    const int NUM_POS_AXIS              = 3;
    const int NUM_ROT_AXIS              = 4;
    static readonly string[] AXIS_NAMES = new string[] {"x", "y", "z", "w"};
    static readonly Dictionary<string, string> MecanimMap = new Dictionary<string, string>()
        {
            { "Chest",          "Neck"},
            { "Head",           "Head"},
            { "Hips",           "Root"},
            { "LeftFoot",       "Left Foot"},
            { "LeftHand",       "Left Wrist"},
            { "LeftLowerArm",   "Left Elbow"},
            { "LeftLowerLeg",   "Left Knee"},
            //{ "LeftShoulder",   "Left Shoulder 1"},
            { "LeftUpperArm",   "Left Shoulder 2"},
            { "LeftUpperLeg",   "Left Hip"},
            { "RightFoot",      "Right Foot"},
            { "RightHand",      "Right Wrist"},
            { "RightLowerArm",  "Right Elbow"},
            { "RightLowerLeg",  "Right Knee"},
            //{ "RightShoulder",  "Right Shoulder 1"},
            { "RightUpperArm",  "Right Shoulder 2"},
            { "RightUpperLeg",  "Right Hip"},
            { "Spine",          "Waist"}
        };


    // Builds a skelation for the given mesh
    public static TS2Bone[] BuildTS2Skelation(TS2.Model Model)
    {
        var bones = new List<TS2Bone>();

        for (short i = 0; i < Model.MeshInfos.Length; i++)
        {
            var meshInfo     = Model.MeshInfos[i];
            var mesh         = Model.Meshes[i];
            var isRootOrBone = i == 0 || meshInfo.IsBone > 0;

            if (isRootOrBone)
            {
                var bone = new TS2Bone()
                {
                    ID           = meshInfo.IsBone,
                    Position     = Vector3.zero,
                    Rotation     = Quaternion.identity,
                    MeshSections = new List<short>()
                };

                bone.MeshSections.Add(i);

                if (mesh.MainMesh != null)
                {
                    var points    = mesh.MainMesh.Verts.Select(x => TSMeshUtils.Ts2VertToV3(x)).ToList();
                    bone.Position = Utils.GetCenterOfPoints(points);
                    bones.Add(bone);
                }
            }
        }

        foreach (var bone in bones)
        {
            var meshSectionIdx = bone.MeshSections[0];
            while (true)
            {
                var meshInfo = Model.MeshInfos[meshSectionIdx];

                if (meshInfo.HasChild)
                {
                    var childMeshInfo = Model.MeshInfos[meshInfo.ChildIdx];
                    if (childMeshInfo.IsBone == 0)
                    {
                        bone.MeshSections.Add(meshInfo.ChildIdx);
                        meshSectionIdx = meshInfo.ChildIdx;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        return bones.ToArray();
    }

    public static Transform[] CreateModelBindPose(GameObject Gobj, TS2ModelInfo TS2ModelInfo, Mesh Mesh, float ModelScale)
    {
        var bones            = new List<Transform>();
        var bonePaths        = new string[TS2ModelInfo.Skeleton.Value.BoneMap.Length];
        var bindPoseAnimData = TSAssetManager.LoadFile(TS2ModelInfo.Skeleton.Value.BindPosePath);
        var bindPoseAnim     = new TS2.Animation(bindPoseAnimData);
        var scale            = new Vector3(ModelScale, 1, 1);

        var bindPoses = new List<Matrix4x4>();
        var matrices  = GetBindPose(bindPoseAnim, TS2ModelInfo.Skeleton.Value, scale);

        for (int i = 0; i < matrices.Length; i++)
        {
            //var mat      = matrices[i];
            var posAndRot  = GetPosAndRotFromRootFrame(bindPoseAnim.BindPose[i]);
            var name       = i == 0 ? "Root" : TS2ModelInfo.Skeleton.Value.Names[i-1];
            var boneName   = $"{name}";
            var bone       = new GameObject(boneName) { hideFlags = HideFlags.DontSave }.transform;

            if (i == 0)
            {
                bone.parent     = Gobj.transform;
                bone.localScale = scale;
                bone.rotation   = Quaternion.identity;
            }

            bones.Add(bone);
        }

        var objRot = Gobj.transform.rotation;
        for (int i = 0; i < TS2ModelInfo.Skeleton.Value.BoneMap.Length; i++)
        {
            var mat       = matrices[i];
            var childern  = TS2ModelInfo.Skeleton.Value.BoneMap[i];
            var posAndRot = GetPosAndRotFromRootFrame(bindPoseAnim.BindPose[i]);

            bones[i].localPosition = posAndRot.Pos;
            bones[i].localRotation = posAndRot.Rot;

            if (i == 0)
            {
                bones[i].localPosition *= ModelScale;
            }

            var bindPose = bones[i].worldToLocalMatrix * Gobj.transform.localToWorldMatrix;
            bindPoses.Add(bindPose);

            foreach (var childIdx in childern)
            {
                bones[childIdx + 1].parent = bones[i];
            }
        }

        Gobj.transform.rotation = objRot;
    
        Mesh.bindposes = bindPoses.ToArray();

        return bones.ToArray();
    }

    public static AnimationClip ConvertAnimation(TS2.Animation Anim, TS2AnimationData.SkelData SkelData, string Name, float PlaybackScale = 0.05f, bool IsLegacy = true, bool UseRootMotion = false, bool IsLooping = true)
    {
        var bonesToMecanim = MecanimMap.ToDictionary(x => x.Value, x => x.Key);
        var animClip = new AnimationClip()
        {
            legacy    = IsLegacy,
            name      = Name,
            wrapMode  = IsLooping ? WrapMode.Loop : WrapMode.Default,
            hideFlags = HideFlags.DontSave
        };

        // Root frame track
        var rootBoneName = IsLegacy ? "Root" : bonesToMecanim["Root"];
        RootTrackToCurves(ref animClip, Anim.RootFrames, rootBoneName, PlaybackScale, UseRootMotion);

        // Skip the first frame as it is offten in the wrong position and doesn't look well on looping animations
        for (int i = 1; i < Anim.Frames.Length - 1; i++)
        {
            var frame    = Anim.Frames[i];
            var bonePath = SkelData.BonePaths[i];
            TrackToCurves(ref animClip, frame, bonePath, PlaybackScale);
        }

        animClip.EnsureQuaternionContinuity();
        animClip.AddEvent(new AnimationEvent()
        {
            functionName    = "AnimationFinished",
            time            = animClip.length,
            stringParameter = Name
        });

        return animClip;
    }

    public static void RootTrackToCurves(ref AnimationClip AnimClip, TS2.Animation.RootFrame[] Frames, string BonePath, float PlaybackScale, bool RootMotion = false)
    {
        var numKeyFrames = NUM_POS_AXIS + NUM_ROT_AXIS;
        var kfPosAndRot  = new Keyframe[numKeyFrames][];

        for (int i = 0; i < numKeyFrames; i++)
        {
            kfPosAndRot[i] = new Keyframe[Frames.Length];
        }

        var firstPosAndRot = GetPosAndRotFromRootFrame(Frames[0]);
        for (int i = 0; i < Frames.Length; i++)
        {
            var frame        = Frames[i];
            var posAndRot    = GetPosAndRotFromRootFrame(frame);
            var time         = i * PlaybackScale;
            var inTangent    = i == 0.0f ? 0.0f : 0.5f;
            var outTangent   = i == (Frames.Length - 1) ? 0.0f : 0.5f;

            if (RootMotion)
            {
                kfPosAndRot[0][i] = new Keyframe(time, posAndRot.Pos.x) { inTangent = inTangent, outTangent = outTangent };
                kfPosAndRot[1][i] = new Keyframe(time, posAndRot.Pos.y) { inTangent = inTangent, outTangent = outTangent };
                kfPosAndRot[2][i] = new Keyframe(time, posAndRot.Pos.z) { inTangent = inTangent, outTangent = outTangent };
            }
            else
            {
                // Disable the root animation the animations have by only taking the position from the first frame
                kfPosAndRot[0][i] = new Keyframe(time, firstPosAndRot.Pos.x) { inTangent = inTangent, outTangent = outTangent };
                kfPosAndRot[1][i] = new Keyframe(time, posAndRot.Pos.y) { inTangent = inTangent, outTangent = outTangent };
                kfPosAndRot[2][i] = new Keyframe(time, firstPosAndRot.Pos.z) { inTangent = inTangent, outTangent = outTangent };
            }

            kfPosAndRot[3][i] = new Keyframe(time, posAndRot.Rot.x) { inTangent = inTangent, outTangent = outTangent };
            kfPosAndRot[4][i] = new Keyframe(time, posAndRot.Rot.y) { inTangent = inTangent, outTangent = outTangent };
            kfPosAndRot[5][i] = new Keyframe(time, posAndRot.Rot.z) { inTangent = inTangent, outTangent = outTangent };
            kfPosAndRot[6][i] = new Keyframe(time, posAndRot.Rot.w) { inTangent = inTangent, outTangent = outTangent };
        }

        for (int i = 0; i < Frames.Length; i++)
        {
            for (int aye = 0; aye < NUM_POS_AXIS; aye++)
            {
                var curve = new AnimationCurve(kfPosAndRot[aye]);
                var prop  = $"localPosition.{AXIS_NAMES[aye]}";
                for (int point = 0; point < kfPosAndRot[aye].Length; point++)
                {
                    curve.SmoothTangents(point, 0);
                }
                AnimClip.SetCurve(BonePath, typeof(Transform), prop, curve);
            }

            for (int aye = 0; aye < NUM_ROT_AXIS; aye++)
            {
                var idx   = aye + NUM_POS_AXIS;;
                var curve = new AnimationCurve(kfPosAndRot[idx]);
                var prop  = $"localRotation.{AXIS_NAMES[aye]}";
                for (int point = 0; point < kfPosAndRot[aye].Length; point++)
                {
                    curve.SmoothTangents(point, 0);
                }
                AnimClip.SetCurve(BonePath, typeof(Transform), prop, curve);
            }
        }
    }

    public static void TrackToCurves(ref AnimationClip AnimClip, TS2.Animation.Frame Frame, string BonePath, float PlaybackScale)
    {
        // Positions
        var kfRot = new Keyframe[NUM_ROT_AXIS][];
        var kfPos = new Keyframe[]
        {
            new Keyframe(0f, Frame.X),
            new Keyframe(0f, Frame.Y),
            new Keyframe(0f, Frame.Z)
        };

        for (int i = 0; i < NUM_ROT_AXIS; i++)
        {
            kfRot[i] = new Keyframe[Frame.Rotations.Length];
        }

        // Rotations
        var lastAngle = Vector3.zero;
        for (int i = 0; i < Frame.Rotations.Length; i++)
        {
            var rot        = Frame.Rotations[i];
            var inTangent  = i == 0.0f ? 0.0f : 0.5f;
            var outTangent = i == (Frame.Rotations.Length - 1) ? 0.0f : 0.5f;
            var time       = (i) * PlaybackScale;

            kfRot[0][i] = new Keyframe(time, rot.X) { inTangent = inTangent, outTangent = outTangent };
            kfRot[1][i] = new Keyframe(time, rot.Y) { inTangent = inTangent, outTangent = outTangent };
            kfRot[2][i] = new Keyframe(time, rot.Z) { inTangent = inTangent, outTangent = outTangent };
            kfRot[3][i] = new Keyframe(time, rot.W) { inTangent = inTangent, outTangent = outTangent };
        }

        // Makes the curves and assign then
        for (int i = 0; i < NUM_POS_AXIS; i++)
        {
            var curve = new AnimationCurve(kfPos[i]);
            var prop  = $"localPosition.{AXIS_NAMES[i]}";
            AnimClip.SetCurve(BonePath, typeof(Transform), prop, curve);
        }

        for (int i = 0; i < NUM_ROT_AXIS; i++)
        {
            var curve = new AnimationCurve(kfRot[i]);
            var prop  = $"localRotation.{AXIS_NAMES[i]}";
            for (int point = 0; point < kfRot[i].Length; point++)
            {
                curve.SmoothTangents(point, 0);
            }

            AnimClip.SetCurve(BonePath, typeof(Transform), prop, curve);
        }
    }

    public static Avatar CreateAvatar(GameObject Go)
    {
        string[] humanName = HumanTrait.BoneName;
        HumanBone[] humanBones = new HumanBone[MecanimMap.Count];
        int j = 0;
        int i = 0;
        while (i < humanName.Length) {
            if (MecanimMap.ContainsKey(humanName[i])) {
                HumanBone humanBone = new HumanBone();
                humanBone.humanName = humanName[i];
                humanBone.boneName = MecanimMap[humanName[i]];
                humanBone.limit.useDefaultValues = true;
                humanBones[j++] = humanBone;
            }
            i++;
        }
        
        var root = Go.transform.GetChild(0).gameObject;
        var avatar = AvatarBuilder.BuildHumanAvatar(Go, new HumanDescription()
        {
            human = humanBones
        });

        avatar.hideFlags = HideFlags.DontSave;

        return avatar;
    }

    // Given a Bindpose animation and a skelation bone mapping returns the transforms for each bone
    public static Matrix4x4[] GetBindPose(TS2.Animation Anim, TS2AnimationData.SkelData SkelData, Vector3 Scale)
    {
        var transforms    = new Matrix4x4[SkelData.BoneMap.Length];
        var rootPosAndRot = GetPosAndRotFromRootFrame(Anim.BindPose[0]);
        var rootMat       = GetMatrixForBone(rootPosAndRot) * Matrix4x4.Scale(Scale);
        var rootChildIds  = SkelData.BoneMap[0];

        transforms[0] = rootMat;

        for (int i = 0; i < SkelData.BoneMap.Length; i++)
        {
            var childIds  = SkelData.BoneMap[i];

            foreach (var childId in childIds)
            {
                var offsetChildId         = childId + 1;
                var posAndRot             = GetPosAndRotFromRootFrame(Anim.BindPose[offsetChildId]);
                var mat                   = transforms[i] * GetMatrixForBone(posAndRot);
                transforms[offsetChildId] = mat;
            }
        }

        return transforms;
    }

    public static Matrix4x4 GetMatrixForBone((Vector3 Pos, Quaternion Rot) PosAndRot, Matrix4x4 LastBoneMat = default(Matrix4x4))
    {
        var mat = Matrix4x4.Translate(PosAndRot.Pos) * Matrix4x4.Rotate(PosAndRot.Rot);
        if (LastBoneMat != default(Matrix4x4)) { mat *= LastBoneMat; }

        return mat;
    }

    public static (Vector3 Pos, Quaternion Rot) GetPosAndRotFromRootFrame(TS2.Animation.RootFrame Root)
    {
        var pos = new Vector3(Root.X, Root.Y, Root.Z);
        var rot = new Quaternion(Root.Rotation.X, Root.Rotation.Y, Root.Rotation.Z, Root.Rotation.W);

        return (pos, rot);
    }
}

public struct TS2Bone
{
    public int          ID;
    public Vector3      Position;
    public Quaternion   Rotation;
    public List<short>  MeshSections; // Index of mesh section that belong to this bone
}
