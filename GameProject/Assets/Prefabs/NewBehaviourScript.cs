using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NewBehaviourScript : NetworkBehaviour
{

	// Use this for initialization
	void Start () {
        Debug.Log(GetComponent<NetworkIdentity>().netId);
     }
	
	// Update is called once per frame
	void Update () {
	}
}
