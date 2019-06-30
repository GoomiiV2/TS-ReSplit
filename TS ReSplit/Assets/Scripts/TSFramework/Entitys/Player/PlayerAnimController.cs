using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(TS2AnimationManager))]
public class PlayerAnimController : MonoBehaviour
{
    public AnimationPool HitAnimations = AnimationPool.Create();

    private CharacterController CharController;
    private NavMeshAgent NavAgent; // For bots
    private TS2AnimationManager AnimationManager;

    private MoveState LastMovementState = MoveState.Unknown;

    // Make a global cache for these
    private Dictionary<string, AnimationClip> AnimStore = new Dictionary<string, AnimationClip>();

    void Start()
    {
        GetComponets();

        //PlayAnimation(TS2Data.PlayerAnims.WalkFwd);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PlayHitAnimation();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            PlayAnimation(TS2Data.PlayerAnims.RunFwd);
        }

        PlayMovementAnims();
    }

    public void PlayHitAnimation()
    {
        var anim = HitAnimations.Pick();
        PlayAnimation(anim);
    }

    void PlayMovementAnims()
    {
        var movementState = GetMovementState();
        if (LastMovementState != movementState)
        {
            switch (movementState)
            {
                case MoveState.Still:
                    PlayAnimation(TS2Data.PlayerAnims.IdleScratchHead);
                    break;

                // Walking / running
                case MoveState.Forward:
                    PlayAnimation(TS2Data.PlayerAnims.WalkFwd);
                    break;
            }

            LastMovementState = movementState;
        }
    }

    public void PlayAnimation(string Animation)
    {
        if (AnimStore.TryGetValue(Animation, out AnimationClip Animclip))
        {
            AnimationManager.PlayAnimation(Animclip);
        }
        else
        {
            var animRecord = TS2Data.AnimationDB.PlayerAnimations[Animation];
            var animData   = TSAssetManager.LoadFile(animRecord.Path);
            var ts2Anim    = new TS2.Animation(animData);
            var clip       = TSAnimationUtils.ConvertAnimation(ts2Anim, animRecord.Skeleton.Value, Animation);

            AnimStore.Add(Animation, clip);

            AnimationManager.PlayAnimation(clip);
        }
    }

    private void GetComponets()
    {
        CharController   = GetComponent<CharacterController>();
        NavAgent         = GetComponent<NavMeshAgent>();
        AnimationManager = GetComponent<TS2AnimationManager>();
    }

    private bool IsStill()
    {
        return GetVelocity().magnitude <= 0;
    }

    private MoveDir GetMoveDir()
    {
        const float THRESHOLD = 0.5f;
        var velDir = GetVelocity().normalized;

        if (Vector3.Distance(velDir, transform.forward) <= THRESHOLD) { return MoveDir.Forward; }
        if (Vector3.Distance(velDir, transform.forward) >= THRESHOLD) { return MoveDir.Back; }
        if (Vector3.Distance(velDir, transform.forward) <= THRESHOLD) { return MoveDir.Left; }
        if (Vector3.Distance(velDir, transform.forward) <= THRESHOLD) { return MoveDir.Right; }

        return MoveDir.Forward;
    }

    private Vector3 GetVelocity()
    {
        var vel = NavAgent != null ? NavAgent.velocity : CharController.velocity;
        return vel;
    }

    private MoveState GetMovementState()
    {
        var state = MoveState.Unknown;

        if (IsStill())
        {
            state = MoveState.Still;
            return state;
        }

        var moveDir = GetMoveDir();
        if (moveDir == MoveDir.Forward) { state = MoveState.Forward; }
        if (moveDir == MoveDir.Back)    { state = MoveState.Back; }
        if (moveDir == MoveDir.Left)    { state = MoveState.Left; }
        if (moveDir == MoveDir.Right)   { state = MoveState.Right; }

        return state;
    }

    [Serializable]
    public struct AnimationPoolEntry
    {
        public string Animation;
        public float Chance;
    }

    [Serializable]
    public struct AnimationPool
    {
        public List<AnimationPoolEntry> Entries;

        public static AnimationPool Create()
        {
            var pool = new AnimationPool()
            {
                Entries = new List<AnimationPoolEntry>()
            };

            return pool;
        }

        // Borrowed from https://forum.unity.com/threads/random-numbers-with-a-weighted-chance.442190/#post-2859534
        public string Pick()
        {
            var weightSum = 0f;
            for (int i = 0; i < Entries.Count; ++i)
            {
                weightSum += Entries[i].Chance;
            }
        
            int index = 0;
            int lastIndex = Entries.Count - 1;
            while (index < lastIndex)
            {
                if (UnityEngine.Random.Range(0, weightSum) < Entries[index].Chance)
                {
                    return Entries[index].Animation;
                }

                weightSum -= Entries[index++].Chance;
            }

            return Entries[index].Animation;
        }
    }

    public enum MoveDir
    {
        Forward,
        Back,
        Left,
        Right,
        Unknown
    }

    public enum MoveState
    {
        Forward,
        Back,
        Left,
        Right,
        Falling,
        Still,
        Unknown
    }
}
