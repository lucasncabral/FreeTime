using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public Mission[] missions;
    Enemy enemy;

    public LivingEntity[] playerEntitity;
    Transform[] playerT;
    float[] nextCampCheckTime;
    Vector3[] campPositionOld;
    bool[] isCamping;
    int indexPlayer = 0;
    int removePlayers = 0;


    float timeBetweenCampingChecks = 2;
    float campThresholdDistance = 1.5f;

    Mission currentMission;
    int currentMissionNumber;
    int currentWaveNumber;

    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;
    float timeBetweenSpawn;
    float reasonSpawnerTime;
    float reasonSpawnerCount;

    MapGenerator map;
    bool isDisabled;
    GameUI gameUi;
    
    void Start()
    {
        loadAssets();
        playerEntitity = new LivingEntity[4];
        playerT = new Transform[4];
        nextCampCheckTime = new float[4];
        campPositionOld = new Vector3[4];
        isCamping = new bool[4];        

        map = FindObjectOfType<MapGenerator>();
        gameUi = FindObjectOfType<GameUI>();
        NextWave();
    }

    public void addPlayer(LivingEntity player)
    {
        playerEntitity[indexPlayer] = player;
        playerT[indexPlayer] = player.transform;

        nextCampCheckTime[indexPlayer] = timeBetweenCampingChecks + Time.time;
        campPositionOld[indexPlayer] = playerT[indexPlayer].position;

        indexPlayer++;
    }

    public void RemovePlayer(LivingEntity player)
    {
        int index = Array.IndexOf(playerEntitity, player);
        playerEntitity[index] = null;
        playerT[index] = null;
        nextCampCheckTime[index] = 0;
        campPositionOld[index] = new Vector3(0,0,0);
        removePlayers++;

        if (removePlayers == indexPlayer) {
            isDisabled = true;
            gameUi.OnGameOver();
        }
    }
    

    private void loadAssets()
    {
        currentMissionNumber = PlayerPrefs.GetInt("missionChoose");
        currentMission = missions[currentMissionNumber];
        timeBetweenSpawn = currentMission.timeBetweenSpawns;
        reasonSpawnerCount = currentMission.reasonSpawnerCount;
        reasonSpawnerTime = currentMission.reasonSpawnerTime;
        enemy = currentMission.prefabEnemy;
    }

    private void Update()
    {
        if (!isDisabled)
        {
            for(int i = 0; i < indexPlayer; i++)
            {
                if (playerEntitity[i] != null && Time.time > nextCampCheckTime[i])
                {
                    nextCampCheckTime[i] = Time.time + timeBetweenCampingChecks;
                    isCamping[i] = (Vector3.Distance(playerT[i].position, campPositionOld[i]) < campThresholdDistance);
                    campPositionOld[i] = playerT[i].position;
                }
            }

            if ((enemiesRemainingToSpawn > 0) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + timeBetweenSpawn;

                SpawnEnemy();
            }
        }
    }

    IEnumerator SpawnEnemy(Transform randomTile, float spawnDelay, float tileFlhshSpeed)
    {
        Material tileMat = randomTile.GetComponent<Renderer>().material;
        Color initialColour = Color.white;
        Color flashColour = Color.red;
        float spawnTimer = 0;

        Resize colorSave = randomTile.GetComponent<Resize>();
        while (spawnTimer < spawnDelay) {
            tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlhshSpeed, 1));
            spawnTimer += Time.deltaTime;
            colorSave.color = tileMat.color;
            yield return null;
        }

        colorSave.color = tileMat.color;

        
            Enemy spawnedEnemy = Instantiate(enemy, randomTile.position + Vector3.up, Quaternion.identity) as Enemy;
            Action OnEnemyDeathAction = () => OnEnemyDeath();
            spawnedEnemy.OnDeath += OnEnemyDeathAction;
        
    }
    
    void SpawnEnemy()
    {
        Transform randomTile = map.GetRandomOpenTile();
        doTileEffect(randomTile);
    }

    void doTileEffect(Transform randomTile)
    {
        float spawnDelay = 1;
        float tileFlhshSpeed = 4;


        for(int i = 0; i < indexPlayer; i++)
        {
            if (playerEntitity[i] !=null && isCamping[i])
            {
                randomTile = map.GetTileFromPosition(playerT[i].position);
                isCamping[i] = false;
                break;
            }
        }

        StartCoroutine(SpawnEnemy(randomTile, spawnDelay, tileFlhshSpeed));
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
        currentMission = missions[currentMissionNumber];
        enemiesRemainingToSpawn = (int) Mathf.Ceil(currentMission.enemyCount * reasonSpawnerCount * currentWaveNumber);
        enemiesRemainingAlive = enemiesRemainingToSpawn;
        timeBetweenSpawn *= reasonSpawnerTime;
        gameUi.OnNewWave(currentWaveNumber, enemiesRemainingAlive);
    }

    // Time Between Flags
    public float timeFlags()
    {
        return (missions[currentMissionNumber].enemyCount / (float)(enemiesRemainingAlive + enemiesRemainingToSpawn));
    }

    [System.Serializable]
	public class Mission
    {
        public bool infinit;
        public int enemyCount;
        public float timeBetweenSpawns;
        [Range (0,1)]
        public float reasonSpawnerTime;
        public float reasonSpawnerCount;

        public Enemy prefabEnemy;


    }
}
