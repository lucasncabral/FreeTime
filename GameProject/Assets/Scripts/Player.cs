using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent (typeof (PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity {

    float moveSpeed;
    Camera viewCamera;
    PlayerController controller;
    GunController gunController;

    public Crosshairs crossHairs;
    int gunNumber = 0;

    public GameObject playerUIPrefab;
    private GameObject playerUIInstance; 
    
    // Use this for initialization
    //protected override void Start ()
    public void Start() {
        if (!isLocalPlayer)
            return;

        moveSpeed = PlayerPrefs.GetFloat("PlayerSpeed");
        GameUI gameUI = FindObjectOfType<GameUI>();
        gameUI.playerEntitity = this;
        
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        
        gunController.CmdEquipGun(gunNumber++);

        gameUI.currentGunController = gunController;

        playerUIInstance = Instantiate(playerUIPrefab);
        playerUIInstance.name = playerUIPrefab.name;
        this.crossHairs = playerUIInstance.GetComponent<Crosshairs>();

        CmdAddPlayer();
    }
    

    [Command]
    void CmdAddPlayer()
    {
        FindObjectOfType<Spawner>().addPlayer(this);
    }    

    // Update is called once per frame
    void Update () {
        if (!isLocalPlayer || this.isDead())
        {
            this.crossHairs.enabled = false;
            return;
        }

        // MOVIMENTA O PERSONAGEM COM AS TECLAS
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;

        // TODO AQUI
        controller.Move(moveVelocity);

        // O PERSONAGEM OLHA PARA ONDE O MOUSE ESTIVER
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight());
        float rayDistance;
        if(groundPlane.Raycast(ray, out rayDistance)) {
            Vector3 point = ray.GetPoint(rayDistance);
            Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);
            crossHairs.transform.position = point;
            crossHairs.DetectTargets(ray);
        }

        // ATIRAR
        if(Input.GetMouseButton(0)){
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            gunController.OntriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();

        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            gunController.ChangeFireMod();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            gunController.CmdEquipGun(gunNumber++);
        }

        if(transform.position.y < -10)
        {
            TakeDamage(health);
        }
    }
}
