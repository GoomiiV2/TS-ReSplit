using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
