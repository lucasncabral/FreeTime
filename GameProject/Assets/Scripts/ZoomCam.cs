using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomCam : MonoBehaviour {
    public Transform MainCam;
    public Transform CrossCam;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetAxis("Mouse ScrollWheel") > 0){
            //GetComponent<Camera>().fieldOfView--;
            MainCam.position = new Vector3(MainCam.transform.position.x, MainCam.transform.position.y - .3f, MainCam.transform.position.z + .2f);
            CrossCam.position = new Vector3(CrossCam.transform.position.x, CrossCam.transform.position.y - .3f, CrossCam.transform.position.z + .2f);
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {

            MainCam.position = new Vector3(MainCam.transform.position.x, MainCam.transform.position.y + .3f, MainCam.transform.position.z - .2f);
            CrossCam.position = new Vector3(CrossCam.transform.position.x, CrossCam.transform.position.y + .3f, CrossCam.transform.position.z - .2f);
            // GetComponent<Camera>().fieldOfView++;
        }
    }
}
