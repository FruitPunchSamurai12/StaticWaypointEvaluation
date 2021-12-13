using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] SpawnEffect effectSpawnPrefab;
    [SerializeField] Weapon enemyStartWeapon;
    [SerializeField] Enemy enemyPrefab;


    public bool spawnedThisWave = false;

    public void Spawn()
    {
        spawnedThisWave = true;
        var effect = effectSpawnPrefab.Get<SpawnEffect>(transform.position, Quaternion.identity);
        effect.onEffectDone += ActuallySpawn;
    }

    void ActuallySpawn(SpawnEffect spawnEffect)
    {
        spawnEffect.onEffectDone -= ActuallySpawn;
        Weapon weapon = Instantiate(enemyStartWeapon, transform.position, Quaternion.identity);
        Enemy enemy = Instantiate(enemyPrefab,new Vector3(transform.position.x,0,transform.position.z),Quaternion.identity);
        enemy.SetStartWeapon(weapon);
        
    }

}
