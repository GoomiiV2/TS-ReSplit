using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TS2Data;
using static PlayerAnimController;
using System;

[RequireComponent(typeof(Animation))]
public class TS2AnimationManager : MonoBehaviour
{
    //public AnimatorController AnimatorController;
    private Animation Animation;
    private bool HasCurrentAnimFinished;
    private AnimatedModelV2 Model;
    public Action<string> OnAnimationFinishedCB;

    private string OneShotAnimationName = null;

    void Awake()
    {
        GetComponets();

        Play("");
    }

    void Update()
    {
        
    }

    public void AddAnimationRecord(string Name, AnimationRecord Record, AnimationSlot Slot)
    {
        var clip = LoadRecord(Name, Record, Model?.ModelScale);
        Animation.AddClip(clip, Name);
        var addedClip       = Animation[Name];
        addedClip.blendMode = AnimationBlendMode.Blend;
        addedClip.layer     = (int)Slot;
        addedClip.weight    = 0.5f;
    }

    public void LoadMoveSet(MoveSet MovSet)
    {
        AddAnimationRecord($"{MovSet.SetName}_Fwd",     MovSet.Fwd,     AnimationSlot.Full);
        AddAnimationRecord($"{MovSet.SetName}_Back",    MovSet.Bck,     AnimationSlot.Full);
        AddAnimationRecord($"{MovSet.SetName}_Left",    MovSet.Left,    AnimationSlot.Full);
        AddAnimationRecord($"{MovSet.SetName}_Right",   MovSet.Right,   AnimationSlot.Full);
    }

    public void Play(string AnimName)
    {
        Animation.Play(AnimName);
    }

    public void CrossFade(string AnimName)
    {
        Animation.CrossFade(AnimName, 0.2f, PlayMode.StopSameLayer);
    }

    public void Stop()
    {
        Animation.Stop();
    }

    // TODO: Cache
    public static AnimationClip LoadRecord(string Name, AnimationRecord Record, Vector3? Scale)
    {
        var scale    = Scale ?? Vector3.one;
        var animData = TSAssetManager.LoadFile(Record.Path);
        var ts2Anim  = new TS2.Animation(animData);
        var clip     = TSAnimationUtils.ConvertAnimation(ts2Anim, Record.Skeleton.Value, Name, UseRootMotion: Record.UseRootMotion, Scale: scale);

        return clip;
    }

    public void PlayAnimation(AnimationClip AnimClip, bool OneShot = false)
    {
        AddAnimation(AnimClip);
        Animation.Play(AnimClip.name, PlayMode.StopAll);

        if (OneShot)
        {
            OneShotAnimationName = AnimClip.name;
        }
    }

    private void AddAnimation(AnimationClip AnimClip)
    {
        Animation.AddClip(AnimClip, AnimClip.name);
    }

    public void AnimationFinished(string AnimationName)
    {
        Debug.Log($"Animation {AnimationName} finished.");
        if (OnAnimationFinishedCB != null) { OnAnimationFinishedCB(AnimationName); }
    }

    private void GetComponets()
    {
        Animation = GetComponent<Animation>();
        Model     = GetComponent<AnimatedModelV2>();
    }

    public enum AnimationSlot
    {
        Full,
        UpperBody
    }
}
