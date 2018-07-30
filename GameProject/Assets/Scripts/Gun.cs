﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Gun : NetworkBehaviour {
    public enum FireMode {Auto, Burst, Single};
    public Sprite image;
    public FireMode fireMode;
    int fireModeSelect;
    bool triggerReleasedSinceLastShot;
    public int burstCount;
    int shotsRemainingInBurst;

    public Transform[] projectileSpawn;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity;


    float nextShotTime;

    public GameObject shell;
    public Transform shellEjection;
    MuzzleFlash muzzleFlash;

    Vector3 recoilSmoothDampVelocity;

    public int projectilesPerMag;
    public float reloadTime = .3f;
    public int projectilesRemainingInMag;
    bool isReloading;

    GameUI gameUi;
    bool flagFirstTime = true;

    [SyncVar]
    public NetworkInstanceId parentNetId;
    GameObject parentObject;

    public override void OnStartClient()
    {
        parentObject = ClientScene.FindLocalObject(parentNetId);
        transform.SetParent(parentObject.transform.GetChild(0));
        transform.rotation = parentObject.transform.GetChild(0).transform.rotation;
        parentObject.GetComponent<GunController>().equippedGun = this;
        parentObject.GetComponent<GunController>().updateIndex(this.name);
    }

    private void Start()
    {
        fireModeSelect = 0;
        ChangeFireMod();

        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        projectilesRemainingInMag = parentObject.GetComponent<GunController>().bulletsRemaining();
    }

    private void Awake()
    {
        gameUi = FindObjectOfType<GameUI>();
    }

    private void Update()
    {
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, .1f);

        if(!isReloading && projectilesRemainingInMag == 0)
        {
                CmdReload();
        }
    }

    [Command]
    void CmdShoot(Vector3 projectilePosition, Quaternion rotation)
    {
            Projectile newProjectile = Instantiate(projectile, projectilePosition, rotation) as Projectile;
            newProjectile.gunController = this.transform.parent.transform.parent.GetComponent<GunController>();
            newProjectile.parentNetId = this.GetComponentInChildren<NetworkIdentity>().netId;
            NetworkServer.Spawn(newProjectile.gameObject);

        CmdShell();
            // if (!isReloading && projectilesRemainingInMag != projectilesPerMag)
            CmdOnShoot();
        
    }

    [Command]
    void CmdShell()
    {
        GameObject shellObject = Instantiate(shell, shellEjection.position, shellEjection.rotation) as GameObject;
        NetworkServer.Spawn(shellObject);
    }

    [Command]
    void CmdRecoil()
    {
        RpcDoRecoilEffect();
    }

    [ClientRpc]
    void RpcDoRecoilEffect()
    {
        transform.localPosition -= Vector3.forward * .2f;
    }

    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    [ClientRpc]
    void RpcDoShootEffect()
    {
        muzzleFlash.Activate();
    }

    [Command]
    public void CmdReload()
    {
        RpcDoReloadEffect();
        CmdShell();
    }

    public void Reload()
    {
        if(projectilesRemainingInMag != projectilesPerMag && !isReloading)
        {
            isReloading = true;
            CmdReload();
        }
        isReloading = false;
    }


    [ClientRpc]
    void RpcDoReloadEffect()
    {
        StartCoroutine(AnimateReload());
    }


    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(.2f);

        float reloadSpeed = 1f/reloadTime;
        float percent = 0;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;
        
        while(percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = 4 * (-Mathf.Pow(percent, 2) + percent);
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
        }
        
        transform.localEulerAngles = new Vector3(0, 0, 0);
        projectilesRemainingInMag = projectilesPerMag;

        yield return new WaitForSeconds(.1f);
        isReloading = false;
    }

    public void OnTriggerHolde()
    {
        if (!isReloading && Time.time > nextShotTime && projectilesRemainingInMag > 0)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                    return;
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                    return;
            }


            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                if (projectilesRemainingInMag == 0)
                    break;
                projectilesRemainingInMag--;

                nextShotTime = Time.time + msBetweenShots / 1000;
                CmdShoot(projectileSpawn[i].position, projectileSpawn[i].rotation);
            }

            CmdShell();

            if (!isReloading && projectilesRemainingInMag != projectilesPerMag)
                CmdOnShoot();
            CmdRecoil();
        }
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
    

    public void ChangeFireMod()
    {
        int i = ((fireModeSelect++) % 3);
        string type = "";
        switch (i)
        {
            case 0:
                fireMode = FireMode.Auto;
                type = "Automatic";
                break;
            case 1:
                fireMode = FireMode.Burst;
                type = "Burst";
                break;
            case 2:
                fireMode = FireMode.Single;
                type = "Single";
                break;
        }
        
        if (!flagFirstTime)
        {
            gameUi.OnNewFireMode(type);
        }
        flagFirstTime = false;
    }
}
