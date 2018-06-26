using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour {
    public static int score { get; private set; }
    float lastEnemyKillTime;
    public static int streakCount;
    float streakExpiryTime = 1f;

    private void Start()
    {
        score = 0;
    }

    private void Update()
    {
        if(lastEnemyKillTime + streakExpiryTime < Time.time)
        {
            streakCount = 0;
        }
    }

    public void OnEnemyKilled()
    {
        if (Time.time != lastEnemyKillTime)
        {
            if (Time.time < lastEnemyKillTime + streakExpiryTime)
            {
                streakCount++;
            }
            else
            {
                streakCount = 0;
            }
            lastEnemyKillTime = Time.time;

            score += 3 + 2 * streakCount;
        }
    }

    public void OnFlagCaptured()
    {
        score += 100;
    }
    

}
