using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : PooledMonoBehaviour
{
    public float explosionTimer = 0.5f;

    private void OnEnable()
    {
        ReturnToPool(explosionTimer);
    }

}
