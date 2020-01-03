using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerData PlayerData;
    public PlayerPrefs Prefs;
    public bool IsInFirstPerson = true;

    private CharacterController CharController;
    private GameObject FirstPerson;
    private GameObject ThirdPerson;
    private bool IsCursorLocked = false;

    void Start()
    {
        CharController = GetComponent<CharacterController>();
        FirstPerson    = transform.Find("FP").gameObject;
        ThirdPerson    = transform.Find("TP").gameObject;

        PlayerData = new PlayerData()
        {
            WalkSpeed = 5.0f,
            Gravity   = 20.0f
        };

        Prefs = new PlayerPrefs()
        {
            LookSens   = new Vector2(1.0f, 1.0f),
            InvertLook = false
        };

        SetViewMode(IsInFirstPerson);
    }

    void Update()
    {

    }

    public void FixedUpdate()
    {
        CursorLock();
        HandleMovement();
        HandleLook();
    }

    private void SetViewMode(bool FirstPersonView)
    {
        IsInFirstPerson = FirstPersonView;
        FirstPerson.SetActive (FirstPersonView);
        ThirdPerson.SetActive(!FirstPersonView);
    }

    private void HandleMovement()
    {
        var delta        = Time.fixedDeltaTime;
        Vector2 inputDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // something something, moving diagonaly is faster
        inputDir      *= (PlayerData.WalkSpeed * delta);
        var moveDir    = transform.forward * inputDir.y + transform.right * inputDir.x;
        moveDir       += Physics.gravity * PlayerData.Gravity * delta;

        CharController.Move(moveDir);
    }

    private void HandleLook()
    {
        var delta     = Time.fixedDeltaTime;
        var lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        lookInput   *= Prefs.LookSens;
        lookInput.y *= Prefs.InvertLook ? 1.0f : -1.0f;

        transform.localRotation             *= Quaternion.Euler(0.0f, lookInput.x, 0.0f);
        FirstPerson.transform.localRotation *= Quaternion.Euler(lookInput.y, 0.0f, 0.0f);
    }

    private void CursorLock()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            IsCursorLocked = false;
        }
        else if (Input.GetMouseButtonUp(0)) {
            IsCursorLocked = true;
        }

        if (IsCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

public struct PlayerData
{
    public float WalkSpeed;
    public float Gravity;
}

public struct PlayerPrefs
{
    public Vector2 LookSens;
    public bool InvertLook;
}
