
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagController : MonoBehaviour
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
    bool generatingFlag;
    
    private void Awake()
    {
        map = FindObjectOfType<MapGenerator>();
        spawner = FindObjectOfType<Spawner>();
    }

    // Use this for initialization
    void Start()
    {
        generatingFlag = false;
        playerEntitity = FindObjectOfType<Player>();
        System.Action OnPlayerDeathAction = () => nextWave();
        playerEntitity.OnDeath += OnPlayerDeathAction;

        nextFlagTime = timeBaseBetweenFlags;
        countFlags = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextFlagTime && FindObjectOfType<Flag>() == null && !generatingFlag)
        {
            StartCoroutine("spawnFlag");

        }
    }
    
    IEnumerator spawnFlag()
    {
        generatingFlag = true;
        float spawnDelay = 1;
        float tileFlhshSpeed = 4;

        Transform randomTile = map.GetRandomOpenTile();

        Material tileMat = randomTile.GetComponent<Renderer>().material;
        // Bug de se iniciar enquanto pisca, ele vai entender que a cor initcial é vermelho
        //Color initialColour = tileMat.color;
        Color initialColour = Color.white;
        Color flashColour = Color.blue;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColour, flashColour, Mathf.PingPong(spawnTimer * tileFlhshSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }

        generatingFlag = false;
        Flag currentFlag = Instantiate(flag, randomTile.position + Vector3.up, Quaternion.identity) as Flag;
        currentFlag.GetComponent<Transform>().rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
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
        float time = Time.time + spawner.timeFlags() * timeBaseBetweenFlags + Random.Range(minFlagTime, maxFlagTime) * countFlags;
        return time;
    }
}
