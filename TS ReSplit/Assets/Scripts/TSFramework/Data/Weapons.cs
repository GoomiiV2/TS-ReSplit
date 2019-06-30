using System.Collections.Generic;
using TSData;
using UnityEngine;

namespace TS2Data
{
    public static class WeaponNames
    {
        public const string Uzi = "UZI";
    }
    
    public static class WeaponsDB
    {
        public readonly static Vector3 DefaultFPWeaponPosition = new Vector3(0.177f, -0.229f, 0.32f);

        public static Dictionary<string, WeaponData> Weapons = new Dictionary<string, WeaponData>()
        {
            {
                WeaponNames.Uzi,
                new WeaponData()
                {
                    Key         = WeaponNames.Uzi,
                    FPModelInfo = ModelDB.Get(ModelType.Weapon, WeaponModels.UziFP),
                    Position    = DefaultFPWeaponPosition
                }
            }
        };

        public struct WeaponData
        {
            public string Key;
            public string NameKey   { get { return $"{Key}_NAME"; } }
            public string DescKey	{ get { return $"{Key}_DESC"; } }

            public TS2ModelInfo FPModelInfo;
            public Vector3 Position;
        }
    }
}