using System;
using System.Collections.Generic;
using TSData;

namespace TS2Data
{
    public enum ModelType
    {
        Player,
        Weapon
    }

    public static class PlayerModels
    {
        public const string Viola          = "Viola";
        public const string ReaperSplitter = "ReaperSplitter";
        public const string CorpHart       = "CorpHart";
    };

    public static class WeaponModels
    {
        public const string UziFP               = "Uzi FP";
        public const string UziTP               = "Uzi TP";
        public const string UziPromo            = "Uzi Promo";
        public const string SliencedLugerFP     = "Slienced Luger FP";
        public const string SliencedLugerTP     = "Slienced Luger TP";
        public const string SliencedLugerPromo  = "Slienced Luger Promo";
        public const string SliencedPistolPromo = "Slienced Pistol Promo";
    };

    public static class ModelDB
    {
        // Note: If BoneToMehses is null then an auto maped binging will be created and used
        public static readonly Dictionary<string, TS2ModelInfo>[] Models = new Dictionary<string, TS2ModelInfo>[]
        {
            // Players
            new Dictionary<string, TS2ModelInfo>
            {
                {
                    PlayerModels.Viola,
                    new TS2ModelInfo()
                    {
                        Name         = PlayerModels.Viola,
                        Path         = "ts2/pak/chr.pak/ob/chrs/chr15.raw",
                        SkelType     = TS2AnimationData.SkelationType.Human,
                        BoneToMehses = Bonemap.Create(new short[][]
                        {
                            new short[] { 0, 1 },
                            new short[] { 3 },
                            new short[] { 4 },
                            new short[] { 24 },
                            new short[] { 7 },

                            new short[] { 8 },
                            new short[] { 9, 10, 11, 12 },
                            new short[] { 13, 14 },
                            new short[] { 15 },

                            new short[] { 16 },
                            new short[] { 17, 18, 19, 20 },
                            new short[] { 21, 22 },
                            new short[] { 23 },

                            new short[] { 25, 26 },
                            new short[] { 27, 28 },
                            new short[] { 29, 30 },

                            new short[] { 31, 32 },
                            new short[] { 33, 34 },
                            new short[] { 35, 36 },
                        })
                    }
                },

                {
                    PlayerModels.ReaperSplitter,
                    new TS2ModelInfo()
                    {
                        Name         = PlayerModels.ReaperSplitter,
                        Path         = "ts2/pak/chr.pak/ob/chrs/chr56.raw",
                        SkelType     = TS2AnimationData.SkelationType.Human,
                        BoneToMehses = Bonemap.Create(new short[][]
                        {
                            new short[] { 0, 1 },
                            new short[] { 3 },
                            new short[] { 4 },
                            new short[] { 24 },
                            new short[] { 7 },

                            new short[] { 8 },
                            new short[] { 9, 10, 11, 12 },
                            new short[] { 13, 14 },
                            new short[] { 15 },

                            new short[] { 16 },
                            new short[] { 17, 18, 19, 20 },
                            new short[] { 21, 22 },
                            new short[] { 23 },

                            new short[] { 25, 26 },
                            new short[] { 27, 28 },
                            new short[] { 29, 30 },

                            new short[] { 31, 32 },
                            new short[] { 33, 34 },
                            new short[] { 35, 36 },
                        })
                    }
                },

                {
                    PlayerModels.CorpHart,
                    new TS2ModelInfo()
                    {
                        Name         = PlayerModels.CorpHart,
                        Path         = "ts2/pak/chr.pak/ob/chrs/chr128.raw",
                        SkelType     = TS2AnimationData.SkelationType.Human,
                        BoneToMehses = Bonemap.Create(new short[][]
                        {
                            new short[] { 0, 1 },
                            new short[] { 3 },
                            new short[] { 4 },
                            new short[] { 24 },
                            new short[] { 7 },

                            new short[] { 8 },
                            new short[] { 9, 10, 11, 12 },
                            new short[] { 13, 14 },
                            new short[] { 15 },

                            new short[] { 16 },
                            new short[] { 17, 18, 19, 20 },
                            new short[] { 21, 22 },
                            new short[] { 23 },

                            new short[] { 25, 26 },
                            new short[] { 27, 28 },
                            new short[] { 29, 30 },

                            new short[] { 31, 32 },
                            new short[] { 33, 34 },
                            new short[] { 35, 36 },
                        })
                    }
                },
            },
            
            // Weapons
            new Dictionary<string, TS2ModelInfo>
            {
                {
                    WeaponModels.UziFP,
                    new TS2ModelInfo()
                    {
                        Name         = WeaponModels.UziFP,
                        Path         = "ts2/pak/gun.pak/ob/guns/uzi_ph.raw",
                        SkelType     = TS2AnimationData.SkelationType.None,
                        IngoreMeshes = new int[] { 1 }
                    }
                },
                {
                    WeaponModels.UziTP,
                    new TS2ModelInfo()
                    {
                        Name         = WeaponModels.UziTP,
                        Path         = "ts2/pak/gun.pak/ob/guns/uzi_cl.raw",
                        SkelType     = TS2AnimationData.SkelationType.None,
                        IngoreMeshes = new int[] { }
                    }
                },
                {
                    WeaponModels.SliencedLugerFP,
                    new TS2ModelInfo()
                    {
                        Name         = WeaponModels.SliencedLugerFP,
                        Path         = "ts2/pak/gun.pak/ob/guns/silencedluger_ph.raw",
                        SkelType     = TS2AnimationData.SkelationType.None,
                        IngoreMeshes = new int[] { }
                    }
                },
                {
                    WeaponModels.SliencedLugerTP,
                    new TS2ModelInfo()
                    {
                        Name         = WeaponModels.SliencedLugerTP,
                        Path         = "ts2/pak/gun.pak/ob/guns/silencedluger_cl.raw",
                        SkelType     = TS2AnimationData.SkelationType.None,
                        IngoreMeshes = new int[] { }
                    }
                },
                {
                    WeaponModels.SliencedLugerPromo,
                    new TS2ModelInfo()
                    {
                        Name         = WeaponModels.SliencedLugerPromo,
                        Path         = "ts2/pak/gun.pak/ob/guns/silencedluger_promo.raw",
                        SkelType     = TS2AnimationData.SkelationType.None,
                        IngoreMeshes = new int[] { }
                    }
                },
                {
                    WeaponModels.SliencedPistolPromo,
                    new TS2ModelInfo()
                    {
                        Name         = WeaponModels.SliencedPistolPromo,
                        Path         = "ts2/pak/gun.pak/ob/guns/silencedpistol_promo.raw",
                        SkelType     = TS2AnimationData.SkelationType.None,
                        IngoreMeshes = new int[] { }
                    }
                }
            }
        };
        
        public static TS2ModelInfo Get(ModelType Type, string ModelName)
        {
            var models    = Models[(int)Type];
            var hasModel  = models.TryGetValue(ModelName, out TS2ModelInfo data);
            var modelData = hasModel ? data : null;
            return modelData;
        }
    }
}
