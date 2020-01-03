using TS2Data;
using UnityEngine;

namespace Equipables.TS2
{
    public class SBP90 : EquipableBase
    {
        public new static int ItemID               => (short)WeaponIDs.Uzi;
        public override string FPModelKey          => WeaponModels.UziFP;
        public override Vector3 ViewPos            => new Vector3(0.177f, -0.229f, 0.32f);
        public override Vector3 ProjSpawnPosOffset => new Vector3(0f, 0.0212f, 0.467f);

        public override float PrimaryFireDelay    => 0.0625f;
        public override string PrimaryFireSFXPath => "ts2/pak/sounds.pak/sfx/gun_uzi_withbullet22_01d.vag";

        protected override bool CanShoot => true;

        private Camera ScopeCam;
        private RenderTexture ScopeTex;

        public override void Bind(GameObject Parent)
        {
            base.Bind(Parent);
            CreateScopeCam();
        }

        public override void Unbind()
        {
            base.Unbind();
            DestroyScopeCam();
        }

        public override void Equip(FPWeapon FPWeap, AnimatedModelV2 FPMesh)
        {
            base.Equip(FPWeap, FPMesh);
            FPWeap.ShootAnimSpeedMulti = 16;
            SetScopeTex(FPMesh);
            ScopeCam.gameObject.SetActive(true);
        }

        public override void Unequip()
        {
            base.Unequip();
            ScopeCam.gameObject.SetActive(false);
        }

        public override void PrimaryAction(bool Released)
        {
            base.PrimaryAction(Released);
        }

        private void CreateScopeCam()
        {
            var camGO                          = new GameObject("SBP90 Scope Cam");
            camGO.transform.parent             = ParentGO.transform.Find("FP"); //.Find("Weapon"); // Unity won't let me parent it to the Weapon because, who knows
            ScopeCam                           = camGO.AddComponent<Camera>();
            ScopeCam.transform.localPosition   = new Vector3(0.182f, -0.021f, 0.53f);
            ScopeCam.transform.localRotation   = ParentGO.transform.Find("FP").Find("Weapon").parent.localRotation;
            ScopeCam.transform.localScale      = new Vector3(1, 1, -2);
            ScopeCam.targetDisplay             = 1;
            ScopeCam.fieldOfView               = 25;
            ScopeCam.nearClipPlane             = 0.1f;

            ScopeTex               = new RenderTexture(128, 128, 1);
            ScopeCam.targetTexture = ScopeTex;

            ScopeCam.gameObject.SetActive(false);
        }

        private void SetScopeTex(AnimatedModelV2 Mesh)
        {
            var meshR      = PlayerFPWeapon.gameObject.GetComponent<SkinnedMeshRenderer>();
            var scopeMat   = Resources.Load<Material>("TS2/Weapons/SBP90/Scope");

            int idx = 0;
            foreach (var mat in meshR.materials)
            {
                if (mat.mainTexture.name == "textures/1592.raw")
                {
                    scopeMat.mainTexture = ScopeTex;
                    meshR.materials[idx].CopyPropertiesFromMaterial(scopeMat);
                    meshR.materials[idx].shader = scopeMat.shader;
                }

                idx++;
            }
        }

        private void DestroyScopeCam()
        {
            GameObject.Destroy(ScopeCam.gameObject);
        }
    }
}
