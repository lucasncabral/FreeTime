using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class zzzInstantiatePlayer : MonoBehaviour {
    public Player player;
	// Use this for initialization

	void Start () {
        Instantiate(player, new Vector3(0,0,0), Quaternion.identity);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    
}
