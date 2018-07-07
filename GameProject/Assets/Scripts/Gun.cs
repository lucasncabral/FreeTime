using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Gun : NetworkBehaviour {
    public enum FireMode {Auto, Burst, Single};
    public FireMode fireMode;
    int fireModeSelect;
    bool triggerReleasedSinceLastShot;
    public int burstCount;
    int shotsRemainingInBurst;

    public Transform[] projectileSpawn;
    public GameObject projectile;
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

    public override void OnStartClient()
    {
        GameObject parentObject = ClientScene.FindLocalObject(parentNetId);
        transform.SetParent(parentObject.transform.GetChild(0));
        transform.rotation = parentObject.transform.GetChild(0).rotation;
        parentObject.GetComponent<GunController>().equippedGun = this;
        
    }

    private void Start()
    {
        fireModeSelect = 0;
        ChangeFireMod();
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        //projectilesRemainingInMag = projectilesPerMag;
    }

    private void Awake()
    {
        gameUi = FindObjectOfType<GameUI>();
    }

    private void Update()
    {
        // animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, .1f);

        if(!isReloading && projectilesRemainingInMag == 0)
        {
            Reload();
        }
    }

    [Command]
    void CmdShoot()
    {
        if (!isReloading && Time.time > nextShotTime && projectilesRemainingInMag > 0)
        {
            if(fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                    return;
                shotsRemainingInBurst--;
            }
            else if(fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                    return;
            }


            for (int i=0; i < projectileSpawn.Length; i++) {
                if (projectilesRemainingInMag == 0)
                    break;
                projectilesRemainingInMag--;

            nextShotTime = Time.time + msBetweenShots / 1000;
            GameObject newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation) as GameObject;
            newProjectile.GetComponent<Projectile>().SetSpeed(muzzleVelocity);
            NetworkServer.Spawn(newProjectile);
            }

            GameObject shellObject = Instantiate(shell, shellEjection.position, shellEjection.rotation) as GameObject;
            NetworkServer.Spawn(shellObject);

            muzzleFlash.Activate();
            transform.localPosition -= Vector3.forward * .2f;
        }
    }

    public void Reload()
    {
        if(!isReloading && projectilesRemainingInMag != projectilesPerMag)
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

        isReloading = false;
        projectilesRemainingInMag = projectilesPerMag;
    }

    public void OnTriggerHolde()
    {
        CmdShoot();
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

        if(!flagFirstTime)
        {
            gameUi.OnNewFireMode(type);
        }
        flagFirstTime = false;
    }
}
