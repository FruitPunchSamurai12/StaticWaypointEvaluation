using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] Weapon playerStartWeapon;
    [SerializeField] Transform playerStartPosition;
    [SerializeField] Player player;

    public List<PickUpSpawnPoint> pickUpSpawnPoints = new List<PickUpSpawnPoint>();
    public float timeBetweenPickUpSpawns = 60f;
    float pickupTimer = 0;
    public int pickupsToSpawn = 3;

    public List<SpawnPoint> enemySpawnPoints = new List<SpawnPoint>();
    CellTracker playerCellTracker;

    public float timeBetweenEnemySpawns = 60f;
    public int maxNumberOfEnemiesToSpawn = 5;
    public int minNumberOfEnemiesToSpawn = 2;
    float timer = 0;
    bool callbackReceived = false;
    int waveNumber = 0;
    int numberOfEnemiesToSpawn;
    int currentNumberOfEnemies = 0;
    public int maxNumberOfEnemies = 6;


    bool gameOver = false;
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerCellTracker = player.GetComponent<CellTracker>();
        player.GetComponent<WeaponUser>().EquipWeapon(playerStartWeapon);
        EnemyManager.Instance.onNoEnemiesLeft += SetTimerToSpawnNewWave;
        EnemyManager.Instance.onEnemySpawned += () => currentNumberOfEnemies++; callbackReceived = false;
        EnemyManager.Instance.onEnemyKilled += () => currentNumberOfEnemies--;
        Invoke("StartGame", 1f);
    }

    public void StartGame()
    {
        gameOver = false;
        waveNumber = 0;
        EnemyManager.Instance.Reset();
        UIManager.Instance.Reset();
        ResetAllSpawnPoints();
        player.transform.position = playerStartPosition.position;
        playerStartWeapon.Reset();
        player.Reset();
        timer = timeBetweenEnemySpawns - 5f;
        pickupTimer = 0;
        callbackReceived = true;
        foreach (var pooled in FindObjectsOfType<PooledMonoBehaviour>())
        {
            pooled.gameObject.SetActive(false);
        }
    }

    public void GameOver()
    {
        if (gameOver) return;
        gameOver = true;
        UIManager.Instance.GameOver();
    }

    private void Update()
    {
        if (gameOver) return;
        timer += Time.deltaTime;
        if(timer>timeBetweenEnemySpawns)
        {
            SpawnNewWave();
            timer = 0;
        }
        pickupTimer += Time.deltaTime;
        if(pickupTimer>timeBetweenPickUpSpawns)
        {
            SpawnPickups();
            pickupTimer = 0;
        }
    }

    private void SpawnPickups()
    {
        for (int i = 0; i < pickupsToSpawn; i++)
        {
            int random = Random.Range(0, pickUpSpawnPoints.Count);
            int firstPick = random;
            while(pickUpSpawnPoints[random].spawned)
            {
                random++;
                if (random >= pickUpSpawnPoints.Count)
                    random = 0;
                if (random == firstPick)
                    return;
            }
            bool spawnHealth;
            if (i == 0)
                spawnHealth = false;
            else if (i == 1)
                spawnHealth = true;
            else
                spawnHealth = Random.Range(0, 100) < 50;
            pickUpSpawnPoints[random].Spawn(spawnHealth);
        }
    }

    private void SpawnNewWave()
    {
        DecideOnNumberOfEnemiesToSpawn();
        ResetAllSpawnPoints();
        if (numberOfEnemiesToSpawn > 0)
        {
            for (int i = 0; i < numberOfEnemiesToSpawn; i++)
            {
                int randomSpawnPoint = Random.Range(0, enemySpawnPoints.Count);
                while (enemySpawnPoints[randomSpawnPoint].spawnedThisWave ||
                    enemySpawnPoints[randomSpawnPoint].GetComponent<CellTracker>().cellIndex == playerCellTracker.cellIndex)
                {
                    randomSpawnPoint++;
                    if (randomSpawnPoint >= enemySpawnPoints.Count)
                        randomSpawnPoint = 0;
                }
                enemySpawnPoints[randomSpawnPoint].Spawn();                
            }
            waveNumber++;
        }
    }

    private void ResetAllSpawnPoints()
    {
        foreach (var point in enemySpawnPoints)
        {
            point.spawnedThisWave = false;
        }
    }

    void DecideOnNumberOfEnemiesToSpawn()
    {
        if (waveNumber == 0)
            numberOfEnemiesToSpawn = 2;
        else if (waveNumber == 1)
            numberOfEnemiesToSpawn = 3;
        else
            numberOfEnemiesToSpawn = Random.Range(minNumberOfEnemiesToSpawn, maxNumberOfEnemiesToSpawn);

        if (numberOfEnemiesToSpawn + currentNumberOfEnemies > maxNumberOfEnemies)
            numberOfEnemiesToSpawn = maxNumberOfEnemies - currentNumberOfEnemies;
    }

    void SetTimerToSpawnNewWave()
    {
        if (!callbackReceived)
        {
            timer = timeBetweenEnemySpawns - 2;
            callbackReceived = true;
        }
    }
}
