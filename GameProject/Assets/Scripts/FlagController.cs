
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagController : MonoBehaviour
{
    public Flag flag;
    MapGenerator map;

    public float minFlagTime;
    public float maxFlagTime;
    public float countFlags;
    float nextFlagTime;
    public float timeBaseBetweenFlags;

    Player playerEntitity;
    Spawner spawner;


    private void Awake()
    {
        map = FindObjectOfType<MapGenerator>();
        spawner = FindObjectOfType<Spawner>();
    }

    // Use this for initialization
    void Start()
    {
        playerEntitity = FindObjectOfType<Player>();
        System.Action OnPlayerDeathAction = () => nextWave();
        playerEntitity.OnDeath += OnPlayerDeathAction;

        nextFlagTime = timeBaseBetweenFlags;
        countFlags = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextFlagTime && FindObjectOfType<Flag>() == null)
        {
            Transform randomTile = map.GetRandomOpenTile();
            Instantiate(flag, randomTile.position + Vector3.up, Quaternion.identity);
        }
    }
    

    public void nextWave()
    {
        Flag currentFlag = FindObjectOfType<Flag>();
        nextFlagTime = nextFlagTimeCount();
        if (currentFlag != null)
            currentFlag.nextWave();
    }

    public void captureFlag()
    {
        countFlags++;
        nextWave();
    }

    float nextFlagTimeCount()
    {
        float time = Time.time + spawner.timeFlags() * timeBaseBetweenFlags + Random.Range(minFlagTime, maxFlagTime) * countFlags;
        return time;
    }
}
