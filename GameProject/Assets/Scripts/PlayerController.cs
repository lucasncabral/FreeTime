using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : NetworkBehaviour {

    Vector3 velocity;
    Rigidbody myRigidBody;

    private Transform mainCamera;
    // -8
    private float limitx;
    // -9
    private float limitz;

	void Start () {
        myRigidBody = GetComponent<Rigidbody>();
        
        mainCamera = Camera.main.transform;

        getValues();
        MoveCamera();

	}

    public void getValues()
    {
        MapGenerator map = FindObjectOfType<MapGenerator>();
        limitx = ((map.currentMap.mapSize.x + map.maxMapSize.x) / 4f * map.tileSize) -8f;
        limitz = ((map.currentMap.mapSize.y + map.maxMapSize.y) / 4f * map.tileSize) -9f;
    }

    void MoveCamera()
    {
        if (limitx == 0 && limitz == 0)
            return;
        float x = transform.position.x;
        float y = 9.33f;
        float z = transform.position.z - 12.12f;

        if(transform.position.x > limitx || transform.position.x < -limitx)
        {
            x = Mathf.Sign(transform.position.x) * limitx;
        }
        if (transform.position.z > limitz || transform.position.z < -limitz)
        {
            z = Mathf.Sign(transform.position.z) * limitz - 12.12f;

        }

        mainCamera.position = new Vector3(x, y, z);
    }
	
    void FixedUpdate(){
        if (!isLocalPlayer)
        {
            return;
        }
        
        myRigidBody.MovePosition(myRigidBody.position + velocity * Time.fixedDeltaTime);

        MoveCamera();
    }

    public void Move(Vector3 velocity){
        this.velocity = velocity;
    }

    public void LookAt(Vector3 lookPoint){
        if (!isLocalPlayer)
        {
            return;
        }

        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }
    
}
