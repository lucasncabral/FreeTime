using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class PlayerController : MonoBehaviour {
    public LayerMask groundLayers;
    
    Vector3 velocity;
    Rigidbody myRigidBody;

    float jumpForce = 4f;
    CapsuleCollider col;
    
	void Start () {
        myRigidBody = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
	}
    
	
    void FixedUpdate(){
        myRigidBody.MovePosition(myRigidBody.position + velocity * Time.fixedDeltaTime);
        
    }

    public void Move(Vector3 velocity){
        this.velocity = velocity;
    }

    public void LookAt(Vector3 lookPoint){
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }
    
    public void Jump()
    {
        if (IsGrounded()) {
        myRigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckCapsule(col.bounds.center, new Vector3(col.bounds.center.x, col.bounds.min.y, col.bounds.center.z), col.radius * .5f, groundLayers);
    }
}
