using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
    private Vector3 rotation;
    public Transform weaponHold;

    public Gun[] myGuns;

    // Use this for initialization
    void Start () {
        myGuns = new Gun[3];
	}
	
	// Update is called once per frame
	void Update () {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * 45f);
    }
}
