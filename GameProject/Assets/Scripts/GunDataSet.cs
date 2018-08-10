using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunDataSet : MonoBehaviour
{
    public Gun[] allGuns;

    public int[] preferedGuns;
    public Gun[] usedWeapons;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    // Use this for initialization
    void Start ()
    {

        preferedGuns = new int[3];
        loadGuns();
                
        //allGuns = new Gun[GunBase.Length];
        

        //orderGuns(0);
        
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
        usedWeapons[0] = allGuns[preferedGuns[0]];
        usedWeapons[1] = allGuns[preferedGuns[1]];
        usedWeapons[2] = allGuns[preferedGuns[2]];

        return usedWeapons;
    }

    public void UpdateWeapons(String[] guns)
    {
        int saveGun1 = preferedGuns[0];
        int index = 0;
        foreach (String name in guns)
        {
            preferedGuns[index] = Int32.Parse(name.Substring(name.Length - 3)) - 1;
            index++;
        }

        saveGuns();

        if (saveGun1 != preferedGuns[0])
            FindObjectOfType<GunInventory>().setGunActive();
    }

    /**
    private void orderGuns(int last)
    {
        int j = last;
        foreach (Gun gun in GunBase)
        {
            Debug.Log(gun.name);
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
    **/

    private void loadGuns()
    {
        preferedGuns[0] = PlayerPrefs.GetInt("FirstGun", 0);
        preferedGuns[1] = PlayerPrefs.GetInt("SecondGun", 1);
        preferedGuns[2] = PlayerPrefs.GetInt("ThirdGun", 2);
    }

    private void saveGuns()
    {
        PlayerPrefs.SetInt("FirstGun", preferedGuns[0]);
        PlayerPrefs.SetInt("SecondGun", preferedGuns[1]);
        PlayerPrefs.SetInt("ThirdGun", preferedGuns[2]);
        UsedWeapons();
    }
}
