using TS2Data;
using UnityEngine;

namespace Equipables.TS2
{
    public class SPistol : EquipableBase
    {
        public new static int ItemID               => (short)WeaponIDs.SPistol;
        public override string FPModelKey          => WeaponModels.SliencedPistolPromo;
        public override Vector3 ViewPos            => new Vector3(0.223f, -0.219f, 0.392f);
        public override float PrimaryFireDelay     => 0.5f;
        public override string PrimaryFireSFXPath  => "ts2/pak/sounds.pak/sfx/gun_silenced22.vag";
        public override Vector3 ProjSpawnPosOffset => new Vector3(-0.003f, 0.048f, 0.338f);

        protected override bool CanShoot => true;

        public override void Bind(GameObject Parent)
        {
            base.Bind(Parent);
        }

        public override void Equip(FPWeapon FPWeap, AnimatedModelV2 FPMesh)
        {
            base.Equip(FPWeap, FPMesh);
            FPWeap.ShootAnimSpeedMulti = 2;
        }
    }
}
