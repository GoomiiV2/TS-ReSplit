using System;
using System.Collections;
using System.Collections.Generic;
using TS2Data;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(TS2AnimationManager))]
public class PlayerAnimController : MonoBehaviour
{
    public bool IsPlayerDebug          = false;
    public bool ShouldDrawDebugHelpers       = true;
    public AnimationPool HitAnimations = AnimationPool.Create();

    private CharacterController CharController;
    private NavMeshAgent NavAgent; // For bots
    private TS2AnimationManager AnimationManager;
    private Animation Animation;
    private AnimatedModelV2 Model;

    private MoveState LastMovementState = MoveState.Unknown;
    private MoveSetTypes CurrentMoveSet = MoveSetTypes.Walk;
    private MoveSet[] MoveSets = new MoveSet[]
    {
        new MoveSet()
        {
            SetName       = "Walk",
            CrossFadeDurr = 0.2f,
            Fwd           = AnimationDB.PlayerAnimations[PlayerAnims.WalkFwd],
            Bck           = AnimationDB.PlayerAnimations[PlayerAnims.WalkBack],
            Left          = AnimationDB.PlayerAnimations[PlayerAnims.WalkLeft],
            Right         = AnimationDB.PlayerAnimations[PlayerAnims.WalkRight]
        }
    };

    // Make a global cache for these
    private Dictionary<string, AnimationClip> AnimStore = new Dictionary<string, AnimationClip>();

    void Start()
    {
        GetComponets();

        AnimationManager.AddAnimationRecord("Idle_1", AnimationDB.PlayerAnimations[PlayerAnims.IdleScratchHead], TS2AnimationManager.AnimationSlot.Full);
        AnimationManager.AddAnimationRecord("Idle_2", AnimationDB.PlayerAnimations[PlayerAnims.IdleStand], TS2AnimationManager.AnimationSlot.Full);
        AnimationManager.LoadMoveSet(MoveSets[(int)MoveSetTypes.Walk]);

        //PlayAnimation(TS2Data.PlayerAnims.WalkFwd);
    }

    void AddAnimations()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            PlayHitAnimation();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            PlayAnimation(TS2Data.PlayerAnims.RunFwd);
        }

        PlayMovementAnims();
        if (ShouldDrawDebugHelpers) { DrawDebugHelpers(); }
    }

    public void PlayHitAnimation()
    {
        var anim = HitAnimations.Pick();
        PlayAnimation(anim);
    }

    void PlayMovementAnims()
    {
        var moveSet            = MoveSets[(int)CurrentMoveSet];
        var movementState      = GetMovementState();
        if (LastMovementState != movementState)
        {
            switch (movementState)
            {
                case MoveState.Still:
                    Animation.CrossFade("Idle_1", moveSet.CrossFadeDurr, PlayMode.StopSameLayer);
                    break;

                // Walking / running
                case MoveState.Forward:
                    Animation.CrossFade($"{moveSet.SetName}_Fwd", moveSet.CrossFadeDurr);
                    break;

                case MoveState.Back:
                    Animation.CrossFade($"{moveSet.SetName}_Back", moveSet.CrossFadeDurr);
                    break;

                case MoveState.Left:
                    Animation.CrossFade($"{moveSet.SetName}_Left", moveSet.CrossFadeDurr);
                    break;

                case MoveState.Right:
                    Animation.CrossFade($"{moveSet.SetName}_Right", moveSet.CrossFadeDurr);
                    break;

                default:
                    //AnimationManager.Stop();
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
            var clip       = TSAnimationUtils.ConvertAnimation(ts2Anim, animRecord.Skeleton.Value, Animation, Scale: Model?.ModelScale);

            AnimStore.Add(Animation, clip);

            AnimationManager.PlayAnimation(clip);
        }
    }

    private void GetComponets()
    {
        CharController   = GetComponentInParent<CharacterController>() ?? GetComponent<CharacterController>();
        NavAgent         = GetComponent<NavMeshAgent>() ?? GetComponentInParent<NavMeshAgent>();
        AnimationManager = GetComponent<TS2AnimationManager>();
        Animation        = GetComponent<Animation>();
        Model            = GetComponent<AnimatedModelV2>();
    }

    private bool IsStill()
    {
        /*if (IsPlayerDebug)
        {
            var isStill = (!Input.GetKeyDown(KeyCode.W) || !Input.GetKeyDown(KeyCode.T))
                && (!Input.GetKeyDown(KeyCode.S) || !Input.GetKeyDown(KeyCode.G))
                && (!Input.GetKeyDown(KeyCode.A) || !Input.GetKeyDown(KeyCode.F))
                && (!Input.GetKeyDown(KeyCode.D) || !Input.GetKeyDown(KeyCode.H));
            return isStill;
        }*/

        return GetVelocity().magnitude <= 0;
    }

    private MoveDir GetMoveDir()
    {
        var velDir         = GetVelocity().normalized;
        var forward        = transform.forward;
        forward.y          = 0;
        velDir.y           = 0;
        float headingAngle = Quaternion.LookRotation(forward).eulerAngles.y;
        float velAngle     = Quaternion.LookRotation(velDir).eulerAngles.y;
        var delta          = (headingAngle - velAngle);
        //Debug.Log($"Heading: {headingAngle}, {velAngle}, {delta}");

        if (delta > -45 && delta < 45)      { return MoveDir.Forward; }
        if (delta > 45 && delta < 135)      { return MoveDir.Right; }
        if (delta < 135 && delta < -135)    { return MoveDir.Back; }
        if (delta > -135 && delta < -45)    { return MoveDir.Left; }

        return MoveDir.Unknown;
    }

    private Vector3 GetVelocity()
    {
        if (IsPlayerDebug)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.T)) { return Vector3.forward * 5; }
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.G)) { return Vector3.back * 5; }
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.F)) { return Vector3.left * 5; }
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.H)) { return Vector3.right * 5; }

            return Vector3.back;
        }
        else
        {
            var vel = NavAgent != null ? NavAgent.velocity : CharController.velocity;
            return vel;
        }
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

    private void DrawDebugHelpers()
    {
        var colors = new Color[]
        {
            Color.cyan,
            Color.green,
            Color.blue,
            Color.red,
            Color.black
        };

        /*var moveDir = GetMoveDir();
        var vel     = GetVelocity().normalized;
        Debug.DrawRay(transform.position, vel * 2, colors[(int)moveDir]);*/
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

    public enum MoveSetTypes
    {
        Walk,
        Run,
        Crouch,
        MAX
    };

    [Serializable]
    public struct MoveSet
    {
        public string SetName;
        public float CrossFadeDurr;
        public AnimationRecord Fwd;
        public AnimationRecord Bck;
        public AnimationRecord Left;
        public AnimationRecord Right;
    }
}
