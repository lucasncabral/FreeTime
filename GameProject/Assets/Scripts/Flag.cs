using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Flag : NetworkBehaviour
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
            CmdFlagCaptured();
            nextWave();
        }
    }

    [Command]
    void CmdFlagCaptured() {

        FindObjectOfType<FlagController>().captureFlag();
        score.CmdOnFlagCaptured();
    }

   
}
