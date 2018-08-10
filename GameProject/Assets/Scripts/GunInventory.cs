using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunInventory : MonoBehaviour {
    public DragAndDropCell[] usedGunsContainers;
    public Transform weaponHold;
    GunDataSet dataSet;
    Gun[] usedGuns;
    Gun[] otherGuns;

    public GameObject panelGuns;
    public GameObject prefabGun;

    // Use this for initialization
    void Start()
    {
        dataSet = FindObjectOfType<GunDataSet>();
        setGunUI();
    }

    public void setGunUI()
    {
        int i = 0;
        usedGuns = dataSet.UsedWeapons();
        foreach (Gun gun in usedGuns)
        {
            usedGunsContainers[i].transform.GetChild(0).GetComponent<Image>().sprite =  gun.image;
            i++;
        }

        setGunActive();

        otherGuns = dataSet.BoughtWeapons();
        int index;
        for(index = 0;  index < panelGuns.transform.childCount; index++)
        {
            Destroy(panelGuns.transform.GetChild(index).gameObject);
        }
        
        foreach(Gun g in otherGuns)
        {
            if(g != null) {
                GameObject gun = Instantiate(prefabGun, panelGuns.transform.position, panelGuns.transform.rotation) as GameObject;
                gun.transform.GetChild(0).GetComponent<Image>().sprite = g.image;
                gun.transform.parent = panelGuns.transform;
                gun.transform.position = Vector3.zero;
                gun.transform.localScale = Vector3.one;
            }
        }

  }

    // Update is called once per frame
    void Update () {
		
	}

    public void setGunActive()
    {
        usedGuns = dataSet.UsedWeapons();
        Gun used = Instantiate(usedGuns[0], weaponHold.position, weaponHold.rotation) as Gun;
        used.GetComponent<Gun>().enabled = false;

        try
        {
            Destroy(weaponHold.transform.GetChild(0).gameObject);
        } catch (Exception e)
        {
            Debug.Log(e);
        }

        used.transform.parent = weaponHold;
    }
}
