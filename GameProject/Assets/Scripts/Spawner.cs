﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public Wave[] waves;
    public Enemy enemy;

    LivingEntity playerEntitity;
    Transform playerT;
    float timeBetweenCampingChecks = 2;
    float nextCampCheckTime;
    float campThresholdDistance = 1.5f;
    Vector3 campPositionOld;
    bool isCamping;

    Wave currentWave;
    int currentWaveNumber;

    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    MapGenerator map;
    bool isDisabled;
    GameUI gameUi;

    private void Start()
    {
        playerEntitity = FindObjectOfType<Player>();
        playerT = playerEntitity.transform;

        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        campPositionOld = playerT.position;
        Action OnPlayerDeathAction = () => OnPlayerDeath();
        playerEntitity.OnDeath += OnPlayerDeathAction;

        map = FindObjectOfType<MapGenerator>();
        gameUi = FindObjectOfType<GameUI>();
        NextWave();
    }

    private void Update()
    {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;
                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerT.position;
            }

            if ((enemiesRemainingToSpawn > 0 || currentWave.infinit) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

                StartCoroutine(SpawnEnemy());
            }
        }
    }

    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlhshSpeed = 4;

        Transform randomTile = map.GetRandomOpenTile();
        if (isCamping)
        {
            randomTile = map.GetTileFromPosition(playerT.position);
            isCamping = false;
        }


        Material tileMat = randomTile.GetComponent<Renderer>().material;
        // Bug de se iniciar enquanto pisca, ele vai entender que a cor initcial é vermelho
        //Color initialColour = tileMat.color;
        Color initialColour = Color.white;
        Color flashColour = Color.red;
        float spawnTimer = 0;

        while(spawnTimer < spawnDelay) {
            tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlhshSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }

        Enemy spawnedEnemy = Instantiate(enemy, randomTile.position + Vector3.up, Quaternion.identity) as Enemy;
        Action OnEnemyDeathAction = () => OnEnemyDeath();
        spawnedEnemy.OnDeath += OnEnemyDeathAction;

        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth);
    } 


    void OnPlayerDeath()
    {
        isDisabled = true;
    }

    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;
        if (enemiesRemainingAlive == 0)
            NextWave();
    }

    void NextWave()
    {
        currentWaveNumber++;
        if(currentWaveNumber -1 < waves.Length) { 
        currentWave = waves[currentWaveNumber - 1];
        enemiesRemainingToSpawn = currentWave.enemyCount;
        enemiesRemainingAlive = enemiesRemainingToSpawn;
            gameUi.OnNewWave(currentWaveNumber);
            
            // changeMap
            map.OnNewWave(currentWaveNumber);
            ResetPlayerPosition();

            FindObjectOfType<FlagController>().nextWave();
        }
    }

    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    // Time Between Flags
    public float timeFlags()
    {
        return (waves[currentWaveNumber-1].enemyCount / (float)(enemiesRemainingAlive + enemiesRemainingToSpawn));
    }

    [System.Serializable]
	public class Wave
    {
        public bool infinit;
        public int enemyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
    }
}
