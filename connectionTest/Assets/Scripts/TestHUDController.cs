using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;

[RequireComponent(typeof(NetworkManager))]
public class TestHUDController : MonoBehaviour
{
    public NetworkManager manager;
    public NetworkMatch networkMatch;

    void Awake()
    {
        manager = GetComponent<NetworkManager>();
        networkMatch = gameObject.AddComponent<NetworkMatch>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                manager.StartServer();
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                manager.StartHost();
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                manager.StartClient();
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                Debug.Log("Aqui 1");
                if (manager.matchMaker == null)
                {
                    manager.StartMatchMaker();
                }
                else
                {
                    if (manager.matchInfo == null)
                    {
                        if (manager.matches == null)
                        {
                            if (Input.GetKeyDown(KeyCode.I))
                            {
                                Debug.Log("Aqui 2");

                                networkMatch.CreateMatch("roomName", 4, true, "", "", "", 0, 0, OnMatchCreate);
                                //manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", manager.OnMatchCreate);
                                //manager.mathcName = "Lucas Teste";
                            }
                        }
                    }
                }
            }


            if (NetworkServer.active && NetworkClient.active)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    manager.StopHost();
                }
            }
        }
    }

    public void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (success)
        {
            Debug.Log("Create match succeeded");
            //matchCreated = true;
            Utility.SetAccessTokenForNetwork(matchInfo.networkId, new NetworkAccessToken(extendedInfo));
            NetworkServer.Listen(matchInfo, 9000);
        }
        else
        {
            Debug.LogError("Create match failed");
        }
    }
}