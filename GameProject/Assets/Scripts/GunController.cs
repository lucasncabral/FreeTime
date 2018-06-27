using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun[] allGuns;

    public int[] gunsBullets;
    Gun equippedGun;
    int equippedGunIndex = 0;

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

    void Start()
    {
    }

    private void Update()
    {
        gunsBullets[equippedGunIndex] = equippedGun.projectilesRemainingInMag;
        accuracy = numberHits / (float)numberBullets;
    }

    public void EquipGun(Gun gunToEquip)
    {
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }

        this.equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        this.equippedGun.transform.parent = weaponHold;
        this.equippedGun.projectilesRemainingInMag = gunsBullets[equippedGunIndex];
    }

    public void EquipGun(int gunIndex)
    {
       // Debug.Log(gunIndex % allGuns.Length);
       equippedGunIndex = gunIndex % allGuns.Length;
        EquipGun(allGuns[equippedGunIndex]);
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
        numberBullets++;
    }

    public void moreOnHit()
    {
        numberHits++;
    }
}
