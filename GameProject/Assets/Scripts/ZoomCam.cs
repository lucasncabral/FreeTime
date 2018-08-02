using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomCam : MonoBehaviour {
    public Transform MainCam;
    int direction = 1;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetAxis("Mouse ScrollWheel") > 0){
            GetComponent<Camera>().fieldOfView--;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            GetComponent<Camera>().fieldOfView++;
        }

        if (Input.GetMouseButtonDown(1))
        {
            direction *= -1;
        }


        if (Input.GetMouseButton(1))
        {
            Camera.main.transform.RotateAround(Vector3.zero, Vector3.up * direction, 40 * Time.deltaTime);
        }
    }
}
