using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunDataSet : MonoBehaviour
{
    public Gun[] GunBase;
    public Gun[] allGuns;

    private Gun[] usedWeapons;
    // Use this for initialization
    void Start ()
    {
        GunBase = Resources.FindObjectsOfTypeAll<Gun>();
        allGuns = new Gun[GunBase.Length];
        orderGuns(0);

        usedWeapons = UsedWeapons();
    }
	
    public Gun[] BoughtWeapons()
    {
        Gun[] response = new Gun[allGuns.Length];
        int i = 0;
        foreach(Gun gun in allGuns)
        {
            if(Array.IndexOf(usedWeapons, gun) == -1)
            {
                response[i] = gun;
                i++;
            }
        }
        return response;
    }

    public Gun[] UsedWeapons()
    {
        usedWeapons = new Gun[3];
        usedWeapons[0] = allGuns[0];
        usedWeapons[1] = allGuns[1];
        usedWeapons[2] = allGuns[3];

        return usedWeapons;
    }

    public void UpdateWeapons()
    {

    }

    private void orderGuns(int last)
    {
        int j = last;
        foreach (Gun gun in GunBase)
        {
            if (gun.name.Equals("Gun 00" + (j+1)))
            {
                allGuns[j] = gun;
                j++;
            }
        }
        if (j < GunBase.Length)
        {
          orderGuns(j);
        }
    }
}
