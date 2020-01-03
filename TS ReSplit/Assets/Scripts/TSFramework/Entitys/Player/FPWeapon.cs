using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TS2Data;

public class FPWeapon : MonoBehaviour
{
    public WeaponsDB.WeaponData WeaponData;
    public SwaySettings Sway = new SwaySettings()
    {
        Speed    = 5,
        SwayMult = 2,

        MoveBobScale = 0.01f,
        BobSpeed     = 2.0f,
        BobResetMult = 2.0f
    };
    public AnimationCurve ShootAnim;
    public float ShootAnimSpeedMulti = 1f;
    public float ShootAnimMoveDist   = 0.2f;

    public AnimationCurve ReloadAnim;
    public float ReloadAnimSpeedMulti = 1f;
    public float ReloadAnimAngle      = 45.0f;
    public float ReloadAnimDown       = 0.1f;

    public Texture2D crosshairImage;

    private GameObject PlayerGO;
    private CharacterController CharController;
    public Vector3 InitalWeaponPos;
    private float BobTimelinePos;
    private float LastBobTimelinePos;

    private bool ShouldPlayShootAnmin = false;
    private float ShootTimelinePos;
    private float LastShootTimelinePos;

    private bool ShouldPlayReloadAnmin = false;
    private float ReloadTimelinePos;
    private Quaternion ReloadRot;

    void Start()
    {
        InitalWeaponPos = transform.localPosition;
        GetComponets();
    }

    void Update()
    {
        LookSway();
        MovementSway();
        UpdateShootAnmin();

        if (!ShouldPlayReloadAnmin)
        {
            var bobDelta            = GetMoveBobSample(BobTimelinePos);
            transform.localPosition = InitalWeaponPos + bobDelta;
        }
        else
        {
            ReloadTimelinePos      += Time.deltaTime * ReloadAnimSpeedMulti;
            var pos                 = transform.localPosition;
            pos.y                   = InitalWeaponPos.y - (ReloadAnimDown * ReloadAnim.Evaluate(ReloadTimelinePos)); // TODO: don't eval this twice
            transform.localPosition = pos;

            if (ReloadTimelinePos >= 2.0f)
            {
                ReloadTimelinePos     = 0f;
                ShouldPlayReloadAnmin = false;
            }
        }
    }

    void OnGUI()
    {
        /*float scale = 4f;
        var width   = crosshairImage.width / scale;
        var height  = crosshairImage.height / scale;

        float xMin = (Screen.width / 2) - (width / 2);
        float yMin = (Screen.height / 2) - (height / 2);
        GUI.DrawTexture(new Rect(xMin, yMin, width, height), crosshairImage);*/
    }

    public void PlayShootAnim()
    {
        if (!ShouldPlayShootAnmin)
        {
            ShouldPlayShootAnmin = true;
        }
    }

    public void PlayReloadSingle()
    {
        ShouldPlayReloadAnmin = true;
    }

    private void UpdateShootAnmin()
    {
        if (ShouldPlayShootAnmin)
        {
            ShootTimelinePos    += Time.deltaTime * ShootAnimSpeedMulti;
            LastShootTimelinePos = ShootTimelinePos;
        }
        
        if (ShootTimelinePos > 1f)
        {
            ShootTimelinePos     = 0;
            ShouldPlayShootAnmin = false;
        }
    }

    private void GetComponets()
    {
        PlayerGO       = gameObject.transform.parent.parent.gameObject;
        CharController = PlayerGO.GetComponent<CharacterController>();
    }

    private void LookSway()
    {
        var mouseMovement   = new Vector3(-Input.GetAxis ("Mouse Y"), Input.GetAxis ("Mouse X"), 0);
        mouseMovement       *= Sway.SwayMult;

        if (!ShouldPlayReloadAnmin)
        {
            var swayRot             = Quaternion.Euler(mouseMovement);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, swayRot, Sway.Speed * Time.deltaTime);
        }
        else
        {
            var rRot                = new Quaternion();
            rRot.eulerAngles        = new Vector3(ReloadAnimAngle, 0, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, rRot, ReloadAnim.Evaluate(ReloadTimelinePos));
        }
    }

    private void MovementSway()
    {
        var isMoveing = CharController.velocity.magnitude > 0;

        if (isMoveing)
        {
            BobTimelinePos += Time.deltaTime * Sway.BobSpeed;
            LastBobTimelinePos = BobTimelinePos;
        }
        else if ((Mathf.Ceil(LastBobTimelinePos) - BobTimelinePos) > 0.001f)
        {
            BobTimelinePos += Time.deltaTime * Sway.BobSpeed;
        }
    }

    private Vector3 GetMoveBobSample(float Time)
    {
        var bobAmount = new Vector3(
            Sway.MoveBobX.Evaluate(Time),
            Sway.MoveBobY.Evaluate(Time),
            Sway.MoveBobZ.Evaluate(Time));

        bobAmount   *= Sway.MoveBobScale;

        if (ShouldPlayShootAnmin)
        {
            bobAmount.z += (ShootAnimMoveDist * ShootAnim.Evaluate(ShootTimelinePos));
        }

        return bobAmount;
    }

    [Serializable]
    public struct SwaySettings
    {
        public float Speed;
        public float SwayMult;

        public AnimationCurve MoveBobX;
        public AnimationCurve MoveBobY;
        public AnimationCurve MoveBobZ;
        public float MoveBobScale;
        public float BobSpeed;
        public float BobResetMult;
    }
}
