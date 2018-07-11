using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreKeeper : NetworkBehaviour {
    
    public static int score;
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

        Debug.Log(score);
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
