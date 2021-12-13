using UnityEngine;
using System;
public class WeaponUser:MonoBehaviour
{
    [SerializeField]Transform equippedWeaponPosition;
    [SerializeField] string userTag;
    Weapon equippedWeapon = null;

    public Weapon weapon => equippedWeapon != null ? equippedWeapon : null;
    public Transform EquippedWeaponPosition => equippedWeaponPosition;

    public event Action onNeedReload;
    public event Action onReloadComplete;
    public event Action onEquip;
    public event Action onAmmoPickup;

    public void UseWeapon(Vector3 direction)
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.Attack(direction);
        }
    }


    public void EquipWeapon(Weapon w)
    {
        if (equippedWeapon)
        {
            UnequipWeapon();
        }
        equippedWeapon = w;
        w.onNeedReload += HandleNeedReload;
        w.onReloadComplete += HandleReloadComplete;
        w.Equip(this,userTag);
        w.transform.SetParent(equippedWeaponPosition);
        w.transform.localPosition = Vector3.zero;
        w.transform.localRotation = Quaternion.identity;
        onEquip?.Invoke();
    }

    public void PickUpAmmo(int ammo)
    {
        if(equippedWeapon !=null)
        {
            equippedWeapon.weaponAmmo.AddAmmo(ammo);
            onAmmoPickup?.Invoke();
        }
    }

    public void UnequipWeapon()
    {
        if (equippedWeapon)
        {
            equippedWeapon.onNeedReload -= HandleNeedReload;
            equippedWeapon.onReloadComplete -= HandleReloadComplete;
            equippedWeapon.Unequip();
            equippedWeapon.transform.SetParent(null);
            equippedWeapon = null;
        }
    }

    void HandleNeedReload()
    {
        onNeedReload?.Invoke();
        StartCoroutine(equippedWeapon.Reload());
    }

    void HandleReloadComplete()
    {
        onReloadComplete?.Invoke();
    }

}