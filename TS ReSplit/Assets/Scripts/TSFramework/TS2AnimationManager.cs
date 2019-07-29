using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TS2Data;
using static PlayerAnimController;

[RequireComponent(typeof(Animation))]
public class TS2AnimationManager : MonoBehaviour
{
    //public AnimatorController AnimatorController;
    private Animation Animation;
    private bool HasCurrentAnimFinished;

    void Awake()
    {
        GetComponets();
    }

    void Update()
    {
        
    }

    public void AddAnimationRecord(string Name, AnimationRecord Record, AnimationSlot Slot)
    {
        var clip = LoadRecord(Name, Record);
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
    public static AnimationClip LoadRecord(string Name, AnimationRecord Record)
    {
        var animData = TSAssetManager.LoadFile(Record.Path);
        var ts2Anim  = new TS2.Animation(animData);
        var clip     = TSAnimationUtils.ConvertAnimation(ts2Anim, Record.Skeleton.Value, Name, UseRootMotion: Record.UseRootMotion);

        return clip;
    }

    public void PlayAnimation(AnimationClip AnimClip)
    {
        AddAnimation(AnimClip);
        Animation.Play(AnimClip.name, PlayMode.StopAll);
    }

    private void AddAnimation(AnimationClip AnimClip)
    {
        Animation.AddClip(AnimClip, AnimClip.name);
    }

    public void AnimationFinished(string AnimationName)
    {
        Debug.Log($"Animation {AnimationName} finished.");
    }

    private void GetComponets()
    {
        Animation = GetComponent<Animation>();
    }

    public enum AnimationSlot
    {
        Full,
        UpperBody
    }
}
