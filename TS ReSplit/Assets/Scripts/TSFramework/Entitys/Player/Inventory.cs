using Equipables;
using System;
using System.Collections.Generic;
using TS2Data;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public const int PRE_ALLOC_INV_SIZE       = 10;
    public int EquipedIdx                     = 0;
    public List<EquipableBase> Items          = new List<EquipableBase>();
    public AssestReferances AssestReferances  = new AssestReferances();

    #region Getters
    private EquipableBase EquipedItem => Items[EquipedIdx];

    private FPWeapon _FPAnimation = null;
    private FPWeapon FPAnimation
    {
        get
        {
            if (_FPAnimation == null)
            {
                _FPAnimation = GetComponentInChildren<FPWeapon>();
            }
            return _FPAnimation;
        }
    }

    private AnimatedModelV2 _FPMesh = null;
    private AnimatedModelV2 FPMesh
    {
        get
        {
            if (_FPMesh == null)
            {
                _FPMesh = GetComponentInChildren<AnimatedModelV2>();
            }
            return _FPMesh;
        }
    }
#endregion

#region Inv Management
    public void AddItem(int Id)
    {
        var item = EquipablesDB.CreateFromID(Id);
        AddItem(item);
    }

    public void AddItem(EquipableBase Item)
    {
        Item.Bind(gameObject);
        Items.Add(Item);
    }

    public void RemoveItem(int Id)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].GetItemID == Id)
            {
                Items[i].Unbind();
                Items.RemoveAt(i);
                return;
            }
        }
    }

    // Equip the next weapon in the inventory and cycle around to the first if reaching the end
    public void NextWeapon()
    {
        var prev = EquipedIdx;
        EquipedIdx++;
        if (EquipedIdx >= Items.Count)
        {
            EquipedIdx = 0;
        }

        SwapItem(prev, EquipedIdx);
    }

    public void PrevWeapon()
    {
        var prev = EquipedIdx;
        EquipedIdx--;
        if (EquipedIdx < 0)
        {
            EquipedIdx = Items.Count -1;
        }

        SwapItem(prev, EquipedIdx);
    }

    public void SwapItem(int Prev, int Next)
    {
        if (Prev != Next)
        {
            if (Prev != -1)
            {
                Items[Prev].Unequip();
            }

            Items[Next].Equip(FPAnimation, FPMesh);
        }
    }

    public void PrimaryAction(bool Released)
    {
        if (EquipedItem != null)
        {
            EquipedItem.PrimaryAction(Released);
        }
    }

    public void SecondaryAction(bool Released)
    {
        if (EquipedItem != null)
        {
            EquipedItem.SecondaryAction(Released);
        }
    }

    public void ReloadAction(bool Released)
    {
        if (EquipedItem != null)
        {
            EquipedItem.ReloadAction(Released);
        }
    }

    public void Start()
    {
        CreateTestInventory();
    }

    private void CreateTestInventory()
    {
        AddItem((int)WeaponIDs.Uzi);
        AddItem((int)WeaponIDs.SPistol);

        SwapItem(-1, 0);
    }

    private void UpdateItems()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            item.Update();
        }
    }

    public void Update()
    {
        if (Input.GetButtonDown("NextWeapon")) { NextWeapon(); }
        if (Input.GetButtonDown("PrevWeapon")) { PrevWeapon(); }

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            NextWeapon();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            PrevWeapon();
        }

        if (Input.GetButtonDown("Fire1") || Input.GetMouseButtonDown(0)) { PrimaryAction(false); }
        if (Input.GetButtonUp("Fire1") || Input.GetMouseButtonUp(0)) { PrimaryAction(true); }

        if (Input.GetButtonDown("Fire2") || Input.GetMouseButtonDown(1)) { SecondaryAction(false); }
        if (Input.GetButtonUp("Fire2") || Input.GetMouseButtonUp(1)) { SecondaryAction(true); }

        if (Input.GetButtonDown("Reload")) { ReloadAction(false); }
        if (Input.GetButtonUp("Reload")) { ReloadAction(true); }

        UpdateItems();
    }

#endregion
}

// So i can get acess to assest store content since i can't move that to the resources folder
// I don't like this
[Serializable]
public struct AssestReferances
{
    public GameObject HitSpark;
}
