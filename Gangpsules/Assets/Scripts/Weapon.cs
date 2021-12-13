using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using System;

public class Weapon : MonoBehaviour, I_Interactable
{
    protected bool equipped = false;
    protected string userTag;
    [SerializeField]
    protected Collider col;
    protected bool fired = false;
    protected WeaponUser user = null;
   
    public float fireRate = 0.2f;
    public float reloadTime = 0f;
    public WeaponAmmo weaponAmmo;
    public int magazineSize;
    public int totalMagazines;
    bool reloading = false;

    public event Action onNeedReload;
    public event Action onReloadComplete;

    public virtual Transform LineOfSightStart { get; }

    private void Awake()
    {
        weaponAmmo = new WeaponAmmo(magazineSize,totalMagazines);
    }

    public virtual void Interact(Player p)
    {
        p.EquipWeapon(this);
    }

    public void Equip(WeaponUser weaponUser, string userTag)
    {
        user = weaponUser;
        this.userTag = userTag;
        equipped = true;
        col.enabled = false;
    }

    public virtual void Unequip()
    {
        equipped = false;
        col.enabled = true;
        user = null;
    }

    public virtual void Attack(Vector3 direction)
    {

    }

    protected bool CanFire()
    {
        bool bulletsInMagazine = weaponAmmo.HasBulletsInMagazine();
        if (!bulletsInMagazine && !reloading)
            onNeedReload?.Invoke();
        return !fired && bulletsInMagazine;
    }
    public IEnumerator Reload()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadTime);
        weaponAmmo.Reload();
        reloading = false;
        onReloadComplete?.Invoke();
    }

    public void Reset()
    {
        fired = false;
        reloading = false;
        weaponAmmo.Reset();
    }
}


[System.Serializable]
public class SharedWeapon : SharedVariable<Weapon>
{
    public static implicit operator SharedWeapon(Weapon value) { return new SharedWeapon { Value = value }; }
}


[System.Serializable]
public class WeaponAmmo
{
    public bool infiniteAmmo = false;
    int magazineSize;
    int totalMagazines;


    public int CurrentMagazineBullets { get; private set; }
    public int TotalBulletsRemaining { get; private set; }

    public WeaponAmmo(int magazineSize, int totalMagazines)
    {
        this.magazineSize = magazineSize;
        this.totalMagazines = totalMagazines;
        CurrentMagazineBullets = magazineSize;
        TotalBulletsRemaining = magazineSize * (totalMagazines - 1);
    }

    public void Reset()
    {
        CurrentMagazineBullets = magazineSize;
        TotalBulletsRemaining = magazineSize * (totalMagazines - 1);
        Reload();
    }

    public bool HasBulletsInMagazine()
    {
        return CurrentMagazineBullets > 0 || infiniteAmmo;
    }

    public bool HasBulletsRemaining()
    {
        return TotalBulletsRemaining+CurrentMagazineBullets > 0;
    }

    public void AddAmmo(int amount)
    {
        TotalBulletsRemaining += amount;
    }

    public void Reload()
    {
        if(TotalBulletsRemaining+CurrentMagazineBullets>=magazineSize)
        {
            TotalBulletsRemaining += CurrentMagazineBullets;
            TotalBulletsRemaining -= magazineSize;
            CurrentMagazineBullets = magazineSize;
        }
        else
        {
            CurrentMagazineBullets = CurrentMagazineBullets + TotalBulletsRemaining;
            TotalBulletsRemaining = 0;
        }
    }

    public bool Fire(int numberOfBullets = 1)
    {
        if (infiniteAmmo) return true;
        if(HasBulletsInMagazine())
        {
            CurrentMagazineBullets-= numberOfBullets;
            return true;
        }
        return false;
    }

}