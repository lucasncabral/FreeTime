using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    private ScoreKeeper score;

    private void Start()
    {
        score = FindObjectOfType<ScoreKeeper>();
    }

    public void nextWave()
    {
        GameObject.Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            FlagCaptured();
            nextWave();
        }
    }
    
    void FlagCaptured() {

        FindObjectOfType<FlagController>().captureFlag();
        score.OnFlagCaptured();
    }

   
}
