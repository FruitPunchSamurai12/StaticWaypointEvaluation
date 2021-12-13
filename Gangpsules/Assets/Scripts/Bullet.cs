using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PooledMonoBehaviour
{
    [SerializeField]
    Rigidbody rb;

    public float speed = 8f;
    [SerializeField]
    int damage = 40;

    [SerializeField]
    Explosion explosion;
    [SerializeField]
    Explosion bloodSplatter;
    [SerializeField]
    Collider col;
    [SerializeField]
    MeshRenderer meshRenderer;
    GameObject instigator;
    Vector3 firedFrom;

    private void OnEnable()
    {
        ReturnToPool(5f);
        meshRenderer.enabled = true;
        col.enabled = true;
        rb.constraints = RigidbodyConstraints.None;
    }


    // Update is called once per frame
    void Update()
    {
        rb.velocity = speed * transform.forward * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Health h = collision.collider.GetComponent<Health>();
        if (h != null)
        {
            h.ModifyHealth(-damage, instigator, firedFrom);
            bloodSplatter.Get<Explosion>(transform.position, Quaternion.LookRotation(-transform.forward));
        }
        meshRenderer.enabled = false;
        col.enabled = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        explosion.Get<Explosion>(transform.position,Quaternion.identity);
        ReturnToPool(5f);
    }

    public void Initialize(GameObject instigator,Vector3 firedFrom)
    {
        this.instigator = instigator;
        this.firedFrom = firedFrom;
    }
}
