using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Resize : NetworkBehaviour
{
    [SyncVar]
    public Vector3 localScaleVec;
    // Use this for initialization

    public override void OnStartClient()
    {
        this.transform.localScale = localScaleVec;
       
    }
}
