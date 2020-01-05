using System;
using System.Collections.Generic;
using TSData;

namespace TS2Data
{
    public static class PlayerAnims
    {
        public const string HitReact1        = "HitReact1";
        public const string HitReact2        = "HitReact2";
        public const string WalkFwd          = "WalkFwd";
        public const string WalkBack         = "WalkBack";
        public const string WalkLeft         = "WalkLeft";
        public const string WalkRight        = "WalkRight";
        public const string MoveFwd          = "MoveFwd";
        public const string RunFwd           = "RunFwd";
        public const string IdleScratchHead  = "IdleScratchHead";
        public const string IdleStand        = "IdleStand";
        public const string HitRightShoulder = "HitRightShoulder";
        public const string HitRightLeg      = "HitRightLeg";
        public const string HitRightStormach = "HitRightStormach";
    };

    public class AnimationDB
    {
        public static Dictionary<string,AnimationRecord> PlayerAnimations = new Dictionary<string,AnimationRecord>
        {
            { PlayerAnims.HitReact1,                AnimationRecord.Create("Hit_react1_m0.raw") },
            { PlayerAnims.HitReact2,                AnimationRecord.Create("Hit_react2_m0.raw") },
            { PlayerAnims.WalkFwd,                  AnimationRecord.Create("walk_m0.raw") },
            { PlayerAnims.WalkBack,                 AnimationRecord.Create("walkback_m0.raw") },
            { PlayerAnims.WalkLeft,                 AnimationRecord.Create("walkleft_m0.raw") },
            { PlayerAnims.WalkRight,                AnimationRecord.Create("walkright_m0.raw") },
            { PlayerAnims.MoveFwd,                  AnimationRecord.Create("moveforward_m0.raw") },
            { PlayerAnims.RunFwd,                   AnimationRecord.Create("run_m0.raw") },
            { PlayerAnims.IdleScratchHead,          AnimationRecord.Create("scratchhead.raw", true) },
            { PlayerAnims.IdleStand,                AnimationRecord.Create("standpose_m0.raw", true) },
            { PlayerAnims.HitRightShoulder,         AnimationRecord.Create("rightshoulder_m0.raw") },
            { PlayerAnims.HitRightLeg,              AnimationRecord.Create("rightleg_m0.raw") },
            { PlayerAnims.HitRightStormach,         AnimationRecord.Create("rightstomach_m0.raw") },
        };

        public AnimationRecord this[string AnimID]
        {
            get
            {
                return PlayerAnimations[AnimID];
            }
        }
    }

    public struct AnimationRecord
    {
        public string Path;
        public TS2AnimationData.SkelationType SkelationType;
        public TS2AnimationData.SkelData? Skeleton {
            get
            {
                if (SkelationType < 0) { return null; }

                return TS2AnimationData.Skeletons[(int)SkelationType];
            }
        }
        public bool UseRootMotion;

        public static AnimationRecord Create(string Path, bool UseRootMotion = false)
        {
            var record = new AnimationRecord()
            {
                Path          = $"ts2/pak/anim.pak/anim/data/ts2/{Path}",
                SkelationType = TS2AnimationData.SkelationType.Human,
                UseRootMotion = UseRootMotion
            };

            return record;
        }
    }
}