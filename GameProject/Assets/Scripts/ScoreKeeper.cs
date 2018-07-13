using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreKeeper : NetworkBehaviour {

    [SyncVar]
    public int score = 0;
    float lastEnemyKillTime;
    public static int streakCount;
    float streakExpiryTime = 1f;

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

    [Command]
    public void CmdOnFlagCaptured()
    {
        score += 100;
    }

    

    void setScore(int Score)
    {
        score = Score;
    }


}
