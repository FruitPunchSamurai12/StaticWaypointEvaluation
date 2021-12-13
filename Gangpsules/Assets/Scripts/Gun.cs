using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{

    [SerializeField]
    protected Transform bulletSpawnPosition;
    [SerializeField]
    protected Bullet bullet;
    [SerializeField]
    protected Animation attackAnimation;
    [SerializeField]
    protected ParticleSystem muzzleFlashFX;


    public override Transform LineOfSightStart => bulletSpawnPosition;

    public override void Attack(Vector3 direction)
    {
        if (equipped)
        {
            if (CanFire())
            {
                if (weaponAmmo.Fire())
                {
                    Bullet b =  bullet.Get<Bullet>(bulletSpawnPosition.position, bulletSpawnPosition.rotation);
                    if (userTag == "Player")
                    {
                        b.gameObject.layer = LayerMask.NameToLayer("PlayerBullets");
                    }
                    b.Initialize(user.gameObject,transform.position);
                    fired = true;
                    attackAnimation.Play();
                    muzzleFlashFX.Play();
                    StartCoroutine(FireCooldown());
                }
            }
        }
    }

    IEnumerator FireCooldown()
    {
        yield return new WaitForSeconds(fireRate);
        fired = false;
    }

}
