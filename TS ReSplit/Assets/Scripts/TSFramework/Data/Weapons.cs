using System.Collections.Generic;
using TSData;
using UnityEngine;

namespace TS2Data
{
    public static class WeaponNames
    {
        public const string Uzi              = "UZI";
        public const string Luger            = "SLuger";
        public const string SPistol          = "SPistol";
        public const string PlasmaMachineGun = "PlasmaMachineGun";
    }

    public enum WeaponIDs : short
    {
        Uzi,
        SLuger,
        SPistol,
        PlasmaMachineGun,
        MAX
    }
    
    public static class WeaponsDB
    {
        public readonly static Vector3 DefaultFPWeaponPosition = new Vector3(0.177f, -0.229f, 0.32f);

        public static Dictionary<WeaponIDs, WeaponData> Weapons = new Dictionary<WeaponIDs, WeaponData>()
        {
            {
                WeaponIDs.Uzi,
                new WeaponData(WeaponNames.Uzi, ModelDB.Get(ModelType.Weapon, WeaponModels.UziFP))
            },
            {   
                WeaponIDs.SLuger,
                new WeaponData(WeaponNames.Luger, ModelDB.Get(ModelType.Weapon, WeaponModels.SliencedLugerFP))
            },
            {
                WeaponIDs.SPistol,
                new WeaponData(WeaponNames.SPistol, ModelDB.Get(ModelType.Weapon, WeaponModels.SliencedPistolPromo), new Vector3(0.223f, -0.219f, 0.392f))
            },
            {
                WeaponIDs.PlasmaMachineGun,
                new WeaponData(WeaponNames.PlasmaMachineGun, ModelDB.Get(ModelType.Weapon, WeaponModels.PlasmaMachineGunPromo), new Vector3(0.223f, -0.219f, 0.392f))
            }
        };

        public struct WeaponData
        {
            public string Key;
            public string NameKey   { get { return $"{Key}_NAME"; } }
            public string DescKey	{ get { return $"{Key}_DESC"; } }

            public TS2ModelInfo FPModelInfo;
            public Vector3 Position;

            public WeaponData(string KeyName, TS2ModelInfo WeapModel, Vector3? Pos = null)
            {
                Key         = KeyName;
                FPModelInfo = WeapModel;
                Position    = Pos ?? DefaultFPWeaponPosition;
            }
        }
    }
}