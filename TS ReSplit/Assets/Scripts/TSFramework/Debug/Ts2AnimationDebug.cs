using Assets.Scripts.TSFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Ts2AnimationDebug : MonoBehaviour
{
    public string AnimationPath;
    public bool Reload = false;
    public bool DrawRootPositons;
    public bool DrawBonePositons;
    public bool DrawFrame;
    public bool ShowBoneNames;
    public bool PlayAnimation;

    [Range(0f, 2f)]
    public float PlaybackRate = 0.02f;
    public int CurrentFrame;

    private TS2.Animation Animation;
    private bool lastReloadState = false;
    private DateTime LastAnimationUpdateTime;
    private Matrix4x4[] BindPose;

    void Start()
    {
        var data                = TSAssetManager.LoadFile(AnimationPath);
        Animation               = new TS2.Animation(data);
        LastAnimationUpdateTime = DateTime.Now;

        if (Animation.IsBindPose)
        {
            BindPose = TSAnimationUtils.GetBindPose(Animation, TS2AnimationData.HumanSkel, Vector3.one * 0.5f);
        }
    }

    void Update()
    {
        if (Animation == null || Reload != lastReloadState)
        {
            Start();
            lastReloadState = Reload;
        }

        UpdateAnimationFrame();
    }

    [ExecuteInEditMode]
    void OnDrawGizmos()
    {
        if (!Animation.IsBindPose)
        {
            if (DrawRootPositons)   { DrawRootFrames(); }
            if (DrawBonePositons)   { DrawBonePositions(); }
            if (DrawFrame)          { DrawFramePose(); }
        }
        else
        {
            if (DrawFrame) { DrawTransforms(BindPose); }
        }
    }

    private void UpdateAnimationFrame()
    {
        if (PlayAnimation)
        {
            if (DateTime.Now > LastAnimationUpdateTime.AddSeconds(PlaybackRate))
            {
                CurrentFrame++;

                if (CurrentFrame >= Animation.Head.NumIds)
                {
                    CurrentFrame = 0;
                }

                LastAnimationUpdateTime = DateTime.Now;
            }
        }
    }

    private void DrawRootFrames()
    {
        if (Animation != null)
        {
            for (int i = 0; i < Animation.RootFrames.Length; i++)
            {
                var rootFrame = Animation.RootFrames[i];
                var pos       = transform.position + new Vector3(rootFrame.X, rootFrame.Y, rootFrame.Z);
                var rot       = Ts2QuatToUnity(rootFrame.Rotation);

                Gizmos.color = Color.magenta;
                DrawPosAndDir(pos, rot);
            }
        }
    }

    private void DrawBonePositions()
    {
        if (Animation != null)
        {
            for (int i = 0; i < Animation.Frames.Length; i++)
            {
                var rootFrame = Animation.Frames[i];
                var pos       = transform.position + new Vector3(rootFrame.X, rootFrame.Y, rootFrame.Z);
                var rot       = Ts2QuatToUnity(rootFrame.Rotations[0]);

                Gizmos.color = Color.cyan;
                DrawPosAndDir(pos, rot);
            }
        }
    }

    private void DrawFramePose()
    {
        var root       = Animation.RootFrames[CurrentFrame];
        var rootPos    = transform.position + new Vector3(root.X, root.Y, root.Z);
        var rootRot    = Ts2QuatToUnity(root.Rotation);
        var rootMatrix = Matrix4x4.Translate(rootPos) * Matrix4x4.Rotate(rootRot);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(rootPos, 0.02f);

        DrawPoseFrame(-1, CurrentFrame, rootMatrix);
    }

    private void DrawPoseFrame(int BoneId, int FrameIdx, Matrix4x4 LastBoneTransform)
    {
        var mapIdx = BoneId + 1;
        if (mapIdx < TS2AnimationData.HumanSkel.BoneMap.Length)
        {
            var connections = TS2AnimationData.HumanSkel.BoneMap[mapIdx];
            for (int i = 0; i < connections.Length; i++)
            {
                var connBoneId     = connections[i];
                var boneFrame      = Animation.Frames[connBoneId];
                var pos            = new Vector3(boneFrame.X, boneFrame.Y, boneFrame.Z);
                var rot            = boneFrame.Rotations[FrameIdx];
                var unityRot       = Ts2QuatToUnity(rot);
                var matrix         = Matrix4x4.Translate(pos) * Matrix4x4.Rotate(unityRot);
                var combinedMatrix = LastBoneTransform * matrix;

                Gizmos.color = new Color32(192, 8, 77, 255);
                Gizmos.DrawWireSphere(combinedMatrix.GetColumn(3), 0.02f);

                if (ShowBoneNames)
                {
                    var name = TS2AnimationData.HumanSkel.Names[connBoneId];
                    Handles.Label(combinedMatrix.GetColumn(3), $"({connBoneId}) {name}");
                }

                Gizmos.color = new Color32(255, 76, 212, 255);
                Gizmos.DrawLine(LastBoneTransform.GetColumn(3), combinedMatrix.GetColumn(3));

                DrawPoseFrame(connBoneId, FrameIdx, combinedMatrix);
            }
        }
    }

    private void DrawTransforms(Matrix4x4[] Transforms)
    {
        for (int i = 0; i < Transforms.Length; i++)
        {
            var boneTransForm = Transforms[i];
            var bonePos       = boneTransForm.GetColumn(3);
            var pos           = transform.position + new Vector3(bonePos.x, bonePos.y, bonePos.z);
            Gizmos.DrawWireSphere(pos, 0.02f);
            var name = i == 0 ? "  (0) Root" : $"  ({i - 1}) {TS2AnimationData.HumanSkel.Names[i - 1]}";
            Handles.Label(pos, name);
        }
    }

    private void DrawPosAndDir(Vector3 Pos, Quaternion Dir)
    {
        Gizmos.DrawWireSphere(Pos, 0.02f);
        var ray = new Ray(Pos,  Dir * Vector3.forward);
        Gizmos.DrawRay(ray);
    }

    private Quaternion Ts2QuatToUnity(TS2.Animation.Quaternion Quat)
    {
        var quat = new Quaternion(Quat.X, Quat.Y, Quat.Z, Quat.W);
        return quat;
    }
}
