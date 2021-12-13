using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    private float noMovementThreshold = 0.0001f;
    private const int noMovementFrames = 3;
    Vector3[] previousLocations = new Vector3[noMovementFrames];

    Transform wielder = null;

    Animator hand;

    [SerializeField]
    Animator animator;

    protected bool canFire = true;


    public override void Interact(Player p)
    {
        equipped = true;
        wielder = p.transform;
        col.enabled = false;
        animator.enabled = true;
        p.EquipWeapon(this);
        hand = GetComponentInParent<Animator>();
        //For good measure, set the previous locations
        for (int i = 0; i < previousLocations.Length; i++)
        {
            previousLocations[i] = wielder.transform.position;
        }
    }

    public override void Attack(Vector3 direction)
    {
        if (canFire)
        {
            hand.SetTrigger("Attack");
            canFire = false;
            StartCoroutine("FireCooldown");
        }
    }

    public override void Unequip()
    {
        hand.SetTrigger("Stop");
        hand = null;
        animator.SetBool("Move", false);
        animator.enabled = false;
        base.Unequip();

    }

    IEnumerator FireCooldown()
    {
        yield return new WaitForSeconds(fireRate);
        canFire = true;
    }

    private void Update()
    {
        if(equipped && wielder!=null)
        {
            //Store the newest vector at the end of the list of vectors
            for (int i = 0; i < previousLocations.Length - 1; i++)
            {
                previousLocations[i] = previousLocations[i + 1];
            }
            previousLocations[previousLocations.Length - 1] = wielder.transform.position;

            for (int i = 0; i < previousLocations.Length - 1; i++)
            {
                if (Vector3.Distance(previousLocations[i], previousLocations[i + 1]) >= noMovementThreshold)
                {
                    //The minimum movement has been detected between frames
                    animator.SetBool("Move", true);
                    break;
                }
                else
                {
                    animator.SetBool("Move", false);
                }
            }
        }
    }
}
