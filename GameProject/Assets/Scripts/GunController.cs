using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GunController : NetworkBehaviour
{
    public Transform weaponHold;
    public Gun[] allGuns;

    public int[] gunsBullets;
    public Gun equippedGun;
    
    public int equippedGunIndex = 0;

    private float numberBullets = 1;
    private float numberHits = 1;
    public float accuracy;


    private void Awake()
    {
        gunsBullets = new int[allGuns.Length];
        int i = 0;
        foreach (Gun gun in allGuns)
        {
            gunsBullets[i] = gun.projectilesPerMag;
            i++;
        }
    }

    private void Update()
    {
       if(equippedGun != null) {
            gunsBullets[equippedGunIndex] = equippedGun.projectilesRemainingInMag;
        }

        accuracy = numberHits / (float) numberBullets;
    }

    public int bulletsRemaining()
    {
        return gunsBullets[equippedGunIndex];
    }    

    [Command]
    public void CmdEquipGun(int gunIndex) {
       equippedGunIndex = gunIndex % allGuns.Length;
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }

        Gun gunToEquip = allGuns[equippedGunIndex];

        this.equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        equippedGun.parentNetId = this.GetComponentInChildren<NetworkIdentity>().netId;

        this.equippedGun.projectilesRemainingInMag = gunsBullets[equippedGunIndex];
        
        NetworkServer.SpawnWithClientAuthority(equippedGun.gameObject, FindObjectOfType<Player>().gameObject);
    }

    public void updateIndex(string name)
    {
        for(int i = 0; i < allGuns.Length; i++)
        {
            if ((allGuns[i].name + "(Clone)") == name)
            {
                equippedGunIndex = i;
                break;
            }
        }
    }


    public void OnTriggerHold()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerHolde();
        }
    }

    public void OntriggerRelease()
    {
        if (equippedGun != null)
        {
            equippedGun.OnTriggerRelease();
        }
    }

    public float GunHeight()
    {
        return weaponHold.position.y;
    }

    public void Reload()
    {
        if (equippedGun != null)
        {
            equippedGun.Reload();
        }
    }

    public void ChangeFireMod()
    {
        if (equippedGun != null)
        {
            equippedGun.ChangeFireMod();
        }
    }

    public void moreOneShoot()
    {
        if(isLocalPlayer)
            numberBullets++;
    }

    public void moreOnHit()
    {
        if(isLocalPlayer)
            numberHits++;
    }
}
