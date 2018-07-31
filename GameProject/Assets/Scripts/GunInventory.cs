using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunInventory : MonoBehaviour {
    public GunContainer[] usedGunsContainers;
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

    private void setGunUI()
    {
        int i = 0;
        usedGuns = dataSet.UsedWeapons();
        foreach (Gun gun in usedGuns)
            {
            usedGunsContainers[i].setImage(gun.image);
            i++;
        }

        setGunActive();

        otherGuns = dataSet.BoughtWeapons();
        foreach(Gun g in otherGuns)
        {
            if(g != null) {
            GameObject gun = Instantiate(prefabGun, panelGuns.transform.position, panelGuns.transform.rotation) as GameObject;
            gun.GetComponent<Image>().sprite = g.image;
            gun.transform.parent = panelGuns.transform;
                gun.transform.position = Vector3.zero;
                gun.transform.localScale = Vector3.one;
            }
        }

  }

    // Update is called once per frame
    void Update () {
		
	}

    void setGunActive()
    {
        Gun used = Instantiate(usedGuns[0], weaponHold.position, weaponHold.rotation) as Gun;
        used.GetComponent<Gun>().enabled = false;
        used.transform.parent = weaponHold;
    }
}
