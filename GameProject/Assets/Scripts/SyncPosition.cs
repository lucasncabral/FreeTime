using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

public class SyncPosition : NetworkBehaviour
{

    private GameObject playerBody;
    private Rigidbody physicsRoot;

    void Start()
    {
        playerBody = transform.gameObject;
        physicsRoot = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isServer)
        {
            CmdSyncPos(transform.localPosition, transform.localRotation, playerBody.transform.localRotation, physicsRoot.velocity);
        }
    }
    

    // Send position to the server and run the RPC for everyone, including the server. 
    [Command]
    protected void CmdSyncPos(Vector3 localPosition, Quaternion localRotation, Quaternion bodyRotation, Vector3 velocity)
    {
        RpcSyncPos(localPosition, localRotation, bodyRotation, velocity);
    }

    // For each player, transfer the position from the server to the client, and set it as long as it's not the local player. 
    [ClientRpc]
    void RpcSyncPos(Vector3 localPosition, Quaternion localRotation, Quaternion bodyRotation, Vector3 velocity)
    {
        if (playerBody == null)
        {
            return;
        }

        if (!isLocalPlayer)
        {
            float speed = 5f;
            this.transform.position = Vector3.Lerp(this.transform.position, localPosition, speed);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, localRotation, speed);
            playerBody.transform.localRotation = Quaternion.Lerp(playerBody.transform.localRotation, bodyRotation, speed);
            this.physicsRoot.velocity = Vector3.Lerp(this.physicsRoot.velocity, velocity, speed);
            /**
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            playerBody.transform.localRotation = bodyRotation;
            physicsRoot.velocity = velocity;
             **/
        }
    }
}
