using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
    Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            GameObject.Destroy(gameObject);
            player.getItem();

        }
    }
}
