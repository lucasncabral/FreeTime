using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagController : MonoBehaviour
{
    public Flag flag;

    public MapGenerator map;

    public float minFlagTime;
    public float maxFlagTime;
    float countFlags;
    float nextFlagTime;
    public float timeBaseBetweenFlags;

    Player playerEntitity;
    Spawner spawner;
    public bool generatingFlag;
    
   
    public Transform randomTile;

    void Start()
    {
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
        if (Time.time > nextFlagTime && FindObjectOfType<Flag>() == null && !generatingFlag)
        {
            generatingFlag = true;
            if(map == null)
                map = FindObjectOfType<MapGenerator>();

            SpawnFlag();
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

        Resize colorSave = randomTile.GetComponent<Resize>();
        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlhshSpeed, 1));
            colorSave.color = tileMat.color;
            spawnTimer += Time.deltaTime;
            yield return null;
        }

        colorSave.color = tileMat.color;
        
        Flag currentFlag = Instantiate(flag, randomTile.position, Quaternion.identity) as Flag;
        currentFlag.GetComponent<Transform>().rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
        
        generatingFlag = false;
    }
    
    
    void SpawnFlag()
    {
        randomTile = map.GetRandomOpenTile();
        doTileEffect();
    }
    
    void doTileEffect()
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
        if(spawner == null)
            spawner = FindObjectOfType<Spawner>();

        float time = Time.time + spawner.timeFlags() * timeBaseBetweenFlags + Random.Range(minFlagTime, maxFlagTime) * countFlags;
        //float time = Time.time + 3f;
        return time;
    }
}
