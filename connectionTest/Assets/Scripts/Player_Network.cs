﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class Player_Network : NetworkBehaviour {
    public GameObject firstPersonCharacter;
    public GameObject[] characterModel;

    public override void OnStartLocalPlayer()
    {
        GetComponent<FirstPersonController>().enabled = true;
        firstPersonCharacter.SetActive(true);

        foreach(GameObject g in characterModel)
        {
            g.SetActive(false);
        }
    }

}
