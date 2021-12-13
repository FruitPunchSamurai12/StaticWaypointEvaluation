using UnityEngine;

public class PickUpSpawnPoint : MonoBehaviour
{
    [SerializeField] Pickup healthPrefab;
    [SerializeField] Pickup ammoPrefab;


    public bool spawned = false;

    public void Spawn(bool spawnHealh)
    {
        if (spawned) return;
        spawned = true;
        Pickup pickup;
        if(spawnHealh)
            pickup = healthPrefab.Get<Pickup>(transform.position, Quaternion.identity);
        else
            pickup = ammoPrefab.Get<Pickup>(transform.position, Quaternion.identity);
        pickup.startPosition = transform.position;
        pickup.OnReturnToPool += PickedUp;
    }

    void PickedUp(PooledMonoBehaviour pickup)
    {
        spawned = false;
        pickup.OnReturnToPool -= PickedUp;
    }

}
