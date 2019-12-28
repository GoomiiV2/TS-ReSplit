using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS2Data;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public const int PRE_ALLOC_INV_SIZE = 10;
    public int EquipedIdx               = 0;
    public List<InvItem> Items          = new List<InvItem>();

    #region Getters
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
    public void AddItem(WeaponIDs Id)
    {
        Items.Add(new InvItem()
        {
            ItemID  = Id,
            IsEmpty = false
        });
    }

    public void AddItem(InvItem Itemd)
    {
        // To do for droped pick ups
    }

    public void RemoveItem(WeaponIDs Id)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].ItemID == Id)
            {
                Items.RemoveAt(i);
                return;
            }
        }
    }

    // Equip the next weapon in the inventory and cycle around to the first if reaching the end
    public void NextWeapon()
    {
        EquipedIdx++;
        if (EquipedIdx >= Items.Count)
        {
            EquipedIdx = 0;
        }

        SetItemVisuals();
    }

    public void PrevWeapon()
    {
        EquipedIdx--;
        if (EquipedIdx < 0)
        {
            EquipedIdx = Items.Count;
        }

        SetItemVisuals();
    }

    public void SetItemVisuals()
    {
        var item = Items[EquipedIdx];
        SetItemVisuals(item);
    }

    public void SetItemVisuals(InvItem ItemInfo)
    {
        if (ItemInfo != null)
        {
            var itemData = WeaponsDB.Weapons[ItemInfo.ItemID];
            FPMesh.LoadModel(itemData.FPModelInfo.Path, itemData.FPModelInfo);
            FPAnimation.InitalWeaponPos = itemData.Position;
        }
        else
        {
            // Hide weapon
        }
    }

    public void PrimaryAction(bool Released)
    {

    }

    public void SecondaryAction(bool Released)
    {

    }

    public void Start()
    {
        CreateTestInventory();
    }

    private void CreateTestInventory()
    {
        AddItem(WeaponIDs.Uzi);
        AddItem(WeaponIDs.SLuger);
        AddItem(WeaponIDs.SPistol);
        AddItem(WeaponIDs.PlasmaMachineGun);
    }

    public void Update()
    {
        if (Input.GetButtonDown("NextWeapon")) { NextWeapon(); }
        if (Input.GetButtonDown("PrevWeapon")) { PrevWeapon(); }

        if (Input.GetAxis("Mouse ScrollWheel") > 0) // forward
        {
            NextWeapon();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0) // backwards
        {
            PrevWeapon();
        }
    }

    #endregion
}

public class InvItem
{
    public WeaponIDs ItemID;
    public bool IsEmpty;
}
