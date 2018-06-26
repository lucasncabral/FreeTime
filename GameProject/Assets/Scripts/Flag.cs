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
        score.OnFlagCaptured();
        FindObjectOfType<FlagController>().captureFlag();
    }
}
