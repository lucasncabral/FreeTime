using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Spawner : NetworkBehaviour
{

    public Mission[] missions;
    Enemy enemy;

    LivingEntity playerEntitity;
    Transform playerT;
    float timeBetweenCampingChecks = 2;
    float nextCampCheckTime;
    float campThresholdDistance = 1.5f;
    Vector3 campPositionOld;
    bool isCamping;

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
    
    public override void OnStartServer()
    {
        loadAssets();

        //playerEntitity = FindObjectOfType<Player>();
        //playerT = playerEntitity.transform;

        //nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        //campPositionOld = playerT.position;
        
        //Action OnPlayerDeathAction = () => OnPlayerDeath();
        //playerEntitity.OnDeath += OnPlayerDeathAction;

        map = FindObjectOfType<MapGenerator>();
        gameUi = FindObjectOfType<GameUI>();
        NextWave();
    }

    //private void Awake()
    private void loadAssets()
    {
        currentMissionNumber = PlayerPrefs.GetInt("missionChoose");
        currentMission = missions[currentMissionNumber];
        timeBetweenSpawn = currentMission.timeBetweenSpawns;
        reasonSpawnerCount = currentMission.reasonSpawnerCount;
        reasonSpawnerTime = currentMission.reasonSpawnerTime;
        enemy = currentMission.prefabEnemy;
    }

    /**
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
    **/

    private void Update()
    {
        if (!isServer)
            return;

        if (!isDisabled)
        {
            /**
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCampingChecks;
                isCamping = (Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance);
                campPositionOld = playerT.position;
            }
            **/

            if ((enemiesRemainingToSpawn > 0) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + timeBetweenSpawn;

                CmdSpawnEnemy();
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

        if (isServer)
        {
            Enemy spawnedEnemy = Instantiate(enemy, randomTile.position + Vector3.up, Quaternion.identity) as Enemy;
            Action OnEnemyDeathAction = () => OnEnemyDeath();
            spawnedEnemy.OnDeath += OnEnemyDeathAction;
            NetworkServer.Spawn(spawnedEnemy.gameObject);
        }
    }

    [Command]
    void CmdSpawnEnemy()
    {
        Transform randomTile = map.GetRandomOpenTile();
        doTileEffect(randomTile);
    }

    void doTileEffect(Transform randomTile)
    {
        float spawnDelay = 1;
        float tileFlhshSpeed = 4;

        /**
       if (isCamping)
       {
           randomTile = map.GetTileFromPosition(playerT.position);
           isCamping = false;
       }
       **/

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
   
    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
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
