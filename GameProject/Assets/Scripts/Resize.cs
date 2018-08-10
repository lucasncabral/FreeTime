using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resize : MonoBehaviour
{
    public Vector3 localScaleVec;
    public Color color;

    // Use this for initialization

    void Start()
    {
        this.transform.localScale = localScaleVec;
        if (this.name.Contains("Tile")) {
            color = Color.white;
            this.name = "Tile";
         }
    }

    private void Update()
    {
        if(this.name.Contains("Tile"))
            this.GetComponent<Renderer>().material.color = color;
    }

    /**
    void Start()
    {
        if (this.name == "Tile(Clone)")
        {
            this.name = "Tile" + this.netId;
        }
    }
    **/
}
