using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : PooledMonoBehaviour
{
    //Frequency at which the item will move up and down
    public float verticalBobFrequency = 1f;
    //Distance the item will move up and down
    public float bobbingAmount = 1f;
    public float rotatingSpeed = 360f;
    public Vector3 startPosition;
    enum pickupTypes
    {
        health,
        ammo
    }

    [SerializeField]
    pickupTypes pickupType;

 


    private void Update()
    {
        // Handle bobbing
        float bobbingAnimationPhase = ((Mathf.Sin(Time.time * verticalBobFrequency) * 0.5f) + 0.5f) * bobbingAmount;
        transform.position = startPosition + Vector3.up * bobbingAnimationPhase;

        // Handle rotating
        transform.Rotate(Vector3.up, rotatingSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if(pickupType==pickupTypes.health)
            {
                var h = other.GetComponent<PlayerHealth>();
                if(h!=null)
                {
                    h.ModifyHealth(2, gameObject, transform.position);
                    ReturnToPool();
                }
            }
            else if(pickupType==pickupTypes.ammo)
            {
                var wUser = other.GetComponent<WeaponUser>();
                if(wUser!=null)
                {
                    wUser.PickUpAmmo(18);
                    ReturnToPool();
                }
            }
        }
    }
}
