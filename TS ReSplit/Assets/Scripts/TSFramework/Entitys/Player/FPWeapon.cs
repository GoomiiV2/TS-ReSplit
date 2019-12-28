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

    public Texture2D crosshairImage;

    private GameObject PlayerGO;
    private CharacterController CharController;
    public Vector3 InitalWeaponPos;
    private float BobTimelinePos;
    private float LastBobTimelinePos;
    
    void Start()
    {
        InitalWeaponPos = transform.localPosition;
        GetComponets();
    }

    void Update()
    {
        LookSway();
        MovementSway();
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

    private void GetComponets()
    {
        PlayerGO       = gameObject.transform.parent.parent.gameObject;
        CharController = PlayerGO.GetComponent<CharacterController>();
    }

    private void LookSway()
    {
        var mouseMovement   = new Vector3(-Input.GetAxis ("Mouse Y"), Input.GetAxis ("Mouse X"), 0);
        mouseMovement       *= Sway.SwayMult;
 
        var swayRot             = Quaternion.Euler(mouseMovement);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, swayRot, Sway.Speed * Time.deltaTime);
    }

    private void MovementSway()
    {
        var isMoveing = CharController.velocity.magnitude > 0;
        var animate = true;

        if (isMoveing)
        {
            BobTimelinePos += Time.deltaTime * Sway.BobSpeed;
            LastBobTimelinePos = BobTimelinePos;
        }
        else if ((Mathf.Ceil(LastBobTimelinePos) - BobTimelinePos) > 0.001f)
        {
            //var lerpTime    = Sway.BobResetMult * Time.deltaTime;
            //var bobProgess  = BobTimelinePos - Mathf.Floor(BobTimelinePos);
            //BobTimelinePos = Mathf.SmoothStep(BobTimelinePos, Mathf.Ceil(LastBobTimelinePos), lerpTime);

            BobTimelinePos += Time.deltaTime * Sway.BobSpeed;
        }
        /*else
        {
            BobTimelinePos = Mathf.Ceil(BobTimelinePos) + 0.5f;
            LastBobTimelinePos = Mathf.Ceil(LastBobTimelinePos) + 0.5f;
            animate = false;
        }*/

        if (animate)
        {
            var bobDelta            = GetMoveBobSample(BobTimelinePos);
            transform.localPosition = InitalWeaponPos + bobDelta;
        }
    }

    private Vector3 GetMoveBobSample(float Time)
    {
        var bobAmount = new Vector3(
            Sway.MoveBobX.Evaluate(Time),
            Sway.MoveBobY.Evaluate(Time),
            Sway.MoveBobZ.Evaluate(Time));

        bobAmount *= Sway.MoveBobScale;

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
