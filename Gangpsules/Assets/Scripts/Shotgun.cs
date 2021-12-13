using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    public override void Attack(Vector3 direction)
    {
        if (equipped)
        {
            if (CanFire())
            {
                if (weaponAmmo.Fire(5))
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    Bullet b1 = bullet.Get<Bullet>(bulletSpawnPosition.position, lookRotation);
                    Quaternion q2 = Quaternion.AngleAxis(15f, Vector3.up);
                    Bullet b2 = bullet.Get<Bullet>(bulletSpawnPosition.position, lookRotation * q2);
                    Quaternion q3 = Quaternion.AngleAxis(30f, Vector3.up);
                    Bullet b3 = bullet.Get<Bullet>(bulletSpawnPosition.position, lookRotation * q3);
                    Quaternion q4 = Quaternion.AngleAxis(-15f, Vector3.up);
                    Bullet b4 = bullet.Get<Bullet>(bulletSpawnPosition.position, lookRotation * q4);
                    Quaternion q5 = Quaternion.AngleAxis(-30f, Vector3.up);
                    Bullet b5 = bullet.Get<Bullet>(bulletSpawnPosition.position, lookRotation * q5);
                    if (userTag == "Player")
                    {
                        int layer = LayerMask.NameToLayer("PlayerBullets");
                        b1.gameObject.layer = layer;
                        b2.gameObject.layer = layer;
                        b3.gameObject.layer = layer;
                        b4.gameObject.layer = layer;
                        b5.gameObject.layer = layer;
                    }
                    fired = true;
                    attackAnimation.Play();
                    muzzleFlashFX.Play();
                    StartCoroutine("FireCooldown");
                }
            }
        }
    }
}
