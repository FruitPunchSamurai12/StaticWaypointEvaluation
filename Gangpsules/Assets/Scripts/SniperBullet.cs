using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperBullet : Bullet
{
    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
