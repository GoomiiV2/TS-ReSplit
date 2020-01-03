using Assets.Scripts.TSFramework.Singletons;
using TS2Data;
using UnityEngine;

namespace Equipables
{
    // Base class for items that will be held by the player aka guns
    public class EquipableBase
    {
        public static int ItemID                        { get { return -1; } }
        public int GetItemID                            { get { return ItemID; } }
        public virtual string FPModelKey                { get { return ""; } }
        public virtual Vector3 ViewPos                  { get { return Vector3.zero; } }
        public virtual float PrimaryFireDelay           { get { return 1f; } }
        public virtual string PrimaryFireSFXPath        { get { return "ts2/pak/sounds.pak/sfx/gun_dryfire01_22.vag"; } }
        public virtual float DryFireDelay               { get { return 1f; } }
        public virtual string DryFireFireSFXPath        { get { return "ts2/pak/sounds.pak/sfx/gun_dryfire01_22.vag"; } }
        public virtual float ReloadDelay                { get { return 1f; } }
        public virtual string ReloadSFXPath             { get { return "ts2/pak/sounds.pak/sfx/reload22_01.vag"; } }
        public virtual Vector3 ProjSpawnPosOffset       { get { return Vector3.zero; } }

        protected GameObject ParentGO              = null;
        protected Inventory Inventory              = null;
        protected FPWeapon PlayerFPWeapon          = null;
        protected virtual bool CanShoot            => false;
        protected bool WantsToShoot                = false;
        protected AudioClip DryFireSFX             = null;
        protected AudioClip PrimaryFireSFX         = null;
        protected AudioClip ReloadSFX              = null;
        protected bool IsReloading                 = false;
        protected GameObject ProjectilePrefab      = null;
        protected bool IsEquiped                   = false;
        protected Camera PlayerCam                 = null;
        protected RaycastHit Aimhit;
        protected GameObject ProjSpawnLoc          = null;

        protected TimeLimitedAction DryFireSFXActioner       = null;
        protected TimeLimitedAction PrimaryFireActioner      = null;
        protected TimeLimitedAction ReloadActioner           = null;
        protected TimeLimitedAction MuzzleFlashHideActioner  = null;

        // Called when added to the inventory
        public virtual void Bind(GameObject Parent)
        {
            var fpGO             = Parent.transform.Find("FP").gameObject;
            var weapGO           = fpGO.transform.Find("Weapon").gameObject;
            ParentGO             = Parent;
            Inventory            = Parent.GetComponent<Inventory>();

            DryFireSFXActioner      = new TimeLimitedAction(DryFireDelay, DoDryFire);
            PrimaryFireActioner     = new TimeLimitedAction(PrimaryFireDelay, DoPrimaryAction);
            ReloadActioner          = new TimeLimitedAction(ReloadDelay, DoReload);
            MuzzleFlashHideActioner = new TimeLimitedAction(PrimaryFireDelay, HideMuzzleFlash);

            DryFireSFX         = ReSplit.Audio.GetAudioClip(DryFireFireSFXPath);
            PrimaryFireSFX     = ReSplit.Audio.GetAudioClip(PrimaryFireSFXPath);
            ReloadSFX          = ReSplit.Audio.GetAudioClip(ReloadSFXPath);

            ProjectilePrefab = Resources.Load<GameObject>("TS2/Weapons/Projectiles/BasicProjectile");
            PlayerCam        = fpGO.GetComponent<Camera>();
            ProjSpawnLoc     = weapGO.transform.Find("ProjSpawn").gameObject;
        }

        public virtual void Unbind()
        {

        }

        // When the item is equiped (held in hands)
        public virtual void Equip(FPWeapon FPWeap, AnimatedModelV2 FPMesh)
        {
            IsEquiped = true;
            FPWeap.InitalWeaponPos     = ViewPos;
            PlayerFPWeapon             = FPWeap;

            var fpModelData = ModelDB.Models[(int)ModelType.Weapon][FPModelKey];
            FPMesh.LoadModel(fpModelData.Path, fpModelData);
            ProjSpawnLoc.transform.localPosition = ProjSpawnPosOffset;
        }

        // When the item is up away
        public virtual void Unequip()
        {
            IsEquiped = false;
        }

        public virtual void PrimaryAction(bool Released)
        {
            WantsToShoot = !Released;
        }

        public virtual void SecondaryAction(bool Released)
        {
            if (!Released)
            {
                PrimaryFireActioner.Run();
            }
        }

        public virtual void ReloadAction(bool Released)
        {
            ReloadActioner.Run();
        }

        private void DoDryFire()
        {
            AudioSource.PlayClipAtPoint(DryFireSFX, ParentGO.transform.position);
        }

        public virtual void Update()
        {
            MuzzleFlashHideActioner.Run();
            if (WantsToShoot)
            {
                if (CanShoot)
                {
                    PrimaryFireActioner.Run();
                }
                else
                {
                    DryFireSFXActioner.Run();
                }
            }

            DoAimRaytrace();
        }

        protected virtual void DoAimRaytrace()
        {
            if (IsEquiped)
            {
                Ray ray = PlayerCam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
                Physics.Raycast(ray, out Aimhit);
            }
        }

        public void DoPrimaryAction()
        {
            SpawnProjectile();
            AudioSource.PlayClipAtPoint(PrimaryFireSFX, ProjSpawnLoc.transform.position);
            PlayerFPWeapon.PlayShootAnim();
            ShowMuzzleFlash();
        }

        protected void SpawnProjectile()
        {
            var pos                   = ProjSpawnLoc.transform.position;
            var proj                  = GameObject.Instantiate(ProjectilePrefab).GetComponent<BasicProjectile>();
            proj.transform.position   = pos;
            proj.Owner                = ParentGO;

            var dir = (Aimhit.point - pos).normalized;
            proj.Shoot(dir, 200f, 1f);
        }

        public void ShowMuzzleFlash()
        {
            var rot                              = ProjSpawnLoc.transform.localRotation;
            rot.eulerAngles                      = new Vector3(0f, 0f, Random.Range(0f, 360f));
            ProjSpawnLoc.transform.localRotation = rot;
            ProjSpawnLoc.SetActive(true);
        }

        public void HideMuzzleFlash()
        {
            ProjSpawnLoc.SetActive(false);
        }

        public void DoReload()
        {
            AudioSource.PlayClipAtPoint(ReloadSFX, ParentGO.transform.position);
            PlayerFPWeapon.PlayReloadSingle();
        }
    }
}
