
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FlagController : NetworkBehaviour
{
    public Flag flag;
    MapGenerator map;

    public float minFlagTime;
    public float maxFlagTime;
    float countFlags;
    float nextFlagTime;
    public float timeBaseBetweenFlags;

    Player playerEntitity;
    Spawner spawner;
    public bool generatingFlag;
    
   
    public Transform randomTile;

    public override void OnStartServer()
    {
        map = FindObjectOfType<MapGenerator>();
        spawner = FindObjectOfType<Spawner>();

        generatingFlag = false;

        //playerEntitity = FindObjectOfType<Player>();
        //System.Action OnPlayerDeathAction = () => nextWave();
        //playerEntitity.OnDeath += OnPlayerDeathAction;

        nextFlagTime = timeBaseBetweenFlags;
        countFlags = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer)
            return;

        if (Time.time > nextFlagTime && FindObjectOfType<Flag>() == null && !generatingFlag)
        {
            generatingFlag = true;
            
            CmdSpawnFlag();
        }
    }
    
    IEnumerator spawnFlag(Transform randomTile, float spawnDelay, float tileFlhshSpeed, GameObject player)
    {
       
        Material tileMat = randomTile.GetComponent<Renderer>().material;
        // Bug de se iniciar enquanto pisca, ele vai entender que a cor initcial é vermelho
        // Color initialColour = tileMat.color;

        Color initialColour = Color.white;
        Color flashColour = Color.blue;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlhshSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }

        if (isServer)
        {
            Flag currentFlag = Instantiate(flag, randomTile.position, Quaternion.identity) as Flag;
            currentFlag.GetComponent<Transform>().rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
            NetworkServer.SpawnWithClientAuthority(currentFlag.gameObject, player);
        }
        generatingFlag = false;
    }
    

    [Command]
    void CmdSpawnFlag()
    {
        randomTile = map.GetRandomOpenTile();
        RpcDoTileEffect();
    }

    [ClientRpc]
    void RpcDoTileEffect()
    {
        generatingFlag = true;
        float spawnDelay = 1;
        float tileFlhshSpeed = 4;
        
        StartCoroutine(spawnFlag(randomTile, spawnDelay, tileFlhshSpeed, FindObjectOfType<Player>().gameObject)); 
    }

    public void nextWave()
    {
        countFlags = 0;
        if (generatingFlag) {
            StopCoroutine("spawnFlag");
            generatingFlag = false;
        }
       nextFlag();
    }

    public void captureFlag()
    {
        countFlags++;
        nextFlag();
    }

    void nextFlag()
    {
        Flag currentFlag = FindObjectOfType<Flag>();
        nextFlagTime = nextFlagTimeCount();
        if (currentFlag != null)
            currentFlag.nextWave();
    }

    float nextFlagTimeCount()
    {
        //float time = Time.time + spawner.timeFlags() * timeBaseBetweenFlags + Random.Range(minFlagTime, maxFlagTime) * countFlags;
        float time = Time.time + 3f;
        return time;
    }
}
