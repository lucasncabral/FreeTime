using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GunContainer : MonoBehaviour {
    private Image image;
    
    public int index;

	// Use this for initialization
	void Start () {
        this.image = this.GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setImage(Sprite GunImage)
    {
        this.image = this.GetComponent<Image>();
        image.sprite = GunImage;
    }

    public void selectContainer()
    {
        this.transform.localScale = new Vector3(1.063928f, 1.063928f, 1.063928f);
    }

    public void unselectContainer()
    {
        this.transform.localScale = new Vector3(0.8369287f, 0.8369287f, 0.8369287f);
    }
}
