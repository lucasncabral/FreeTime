using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Resize : NetworkBehaviour
{
    [SyncVar]
    public Vector3 localScaleVec;
    [SyncVar]
    public Color color;

    // Use this for initialization

    public override void OnStartClient()
    {
        this.transform.localScale = localScaleVec;
        if (this.name.Contains("Tile"))
            color = Color.white;
    }

    private void Update()
    {
        if(this.name.Contains("Tile"))
            this.GetComponent<Renderer>().material.color = color;
    }

    void Start()
    {
        if (this.name == "Tile(Clone)")
        {
            this.name = "Tile" + this.netId;
        }
    }
}
