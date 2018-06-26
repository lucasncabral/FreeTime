using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun[] allGuns;
    Gun equippedGun;

    void Start()
    {
    }

    public void EquipGun(Gun gunToEquip)
    {
        if (equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }

        this.equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;
        this.equippedGun.transform.parent = weaponHold;
    }

    public void EquipGun(int gunIndex)
    {
       // Debug.Log(gunIndex % allGuns.Length);
        EquipGun(allGuns[gunIndex % allGuns.Length]);
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
}
