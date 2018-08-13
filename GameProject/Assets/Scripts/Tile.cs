using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
    public bool isOccuped;
    public MapGenerator.Coord coord;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
            isOccuped = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 10)
            isOccuped = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 10)
            isOccuped = false;
    }

}
