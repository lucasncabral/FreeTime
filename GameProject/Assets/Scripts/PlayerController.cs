using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : NetworkBehaviour {

    Vector3 velocity;
    Rigidbody myRigidBody;
    

	void Start () {
        myRigidBody = GetComponent<Rigidbody>();
	}
	
    void FixedUpdate(){
        if (!isLocalPlayer)
        {
            return;
        }


        myRigidBody.MovePosition(myRigidBody.position + velocity * Time.fixedDeltaTime);
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
