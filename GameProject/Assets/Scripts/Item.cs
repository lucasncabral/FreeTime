using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
    Player player;
    bool used = false;

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if (!used) {
            player.getItem();            
            GameObject.Destroy(gameObject);
            }

            used = true;
        }
    }
}
