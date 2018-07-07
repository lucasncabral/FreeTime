using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Crosshairs : NetworkBehaviour
{
    public LayerMask targetMask;
    public SpriteRenderer dot;
    public Color dotHiglightColour;
    Color originalDotColour;

    private void Start()
    {
        Cursor.visible = false;
        originalDotColour = dot.color;
    }
    // Update is called once per frame
    void Update () {      
        transform.Rotate(Vector3.forward * 40 * Time.deltaTime);
	}

    public void DetectTargets(Ray ray)
    {
        if(Physics.Raycast(ray,100, targetMask))
        {
            dot.color = dotHiglightColour;
        } else
        {
            dot.color = originalDotColour;
        }
    }
}
