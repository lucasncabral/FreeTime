using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour {
    
    public int score = 0;
    float lastEnemyKillTime;
    public int streakCount;
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

                if (streakCount >= 1)
                {
                    StartCoroutine(Shake(1.5f, 0.05f));
                }
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

    

    void setScore(int Score)
    {
        score = Score;
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
