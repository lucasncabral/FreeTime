﻿namespace UnityEngine.Networking
{
    [AddComponentMenu("Network/NetworkManagerHUD")]
    [RequireComponent(typeof(NetworkManager))]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class NewHUD : MonoBehaviour
    {
        public NetworkManager manager;

        // Runtime variable
        bool showServer = false;

        void Awake()
        {
            manager = GetComponent<NetworkManager>();
        }

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
            }

            if (NetworkServer.active && NetworkClient.active)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    manager.StopHost();
                }
            }
        }

        bool firstTime = true;
        bool firstTimeB = true;
        void OnGUI()
        {
            if (!NetworkClient.active && !NetworkServer.active && manager.matchMaker == null)
            {
                if(firstTime)
                Debug.Log("Ja foi!");
                firstTime = false;
            }
            else
            {
                // CONECTADO
                if (NetworkServer.active)
                {
                }
                if (NetworkClient.active)
                {
                    if (firstTimeB) { 
                    Debug.Log("Client: address=" + manager.networkAddress + " port=" + manager.networkPort);
                        firstTimeB = false;
                    }
                }
            }

            /**
            if (NetworkClient.active && !ClientScene.ready)
            {
                if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Client Ready"))
                {
                    ClientScene.Ready(manager.client.connection);

                    if (ClientScene.localPlayers.Count == 0)
                    {
                        ClientScene.AddPlayer(0);
                    }
                }
                ypos += spacing;
            }
            **/

            if (NetworkServer.active || NetworkClient.active)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    manager.StopHost();
                }
            }

            // MATCH MAKER
            if (!NetworkServer.active && !NetworkClient.active)
            {
                if (manager.matchMaker == null)
                {
                    if (Input.GetKeyDown(KeyCode.M))
                    {
                        manager.StartMatchMaker();
                    }
                }
                else
                {
                    if (manager.matchInfo == null)
                    {
                        if (manager.matches == null){
                            if (GUI.Button(new Rect(0, 0, 200, 20), "Create Internet Match"))
                                manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true,"","", "",10,0, manager.OnMatchCreate);

                            GUI.Label(new Rect(45, 45, 100, 20), "Room Name:");
                            manager.matchName = GUI.TextField(new Rect(45 + 100, 45, 100, 20), manager.matchName);
                            
                            if (GUI.Button(new Rect(80, 80, 200, 20), "Find Internet Match"))
                                manager.matchMaker.ListMatches(0, 20, "",true,10,0,manager.OnMatchList);
                        }else {
                            foreach (var match in manager.matches)
                            {
                                if (GUI.Button(new Rect(130, 130, 200, 20), "Join Match:" + match.name))
                                {
                                    manager.matchName = match.name;
                                    manager.matchSize = (uint)match.currentSize;
                                    manager.matchMaker.JoinMatch(match.networkId, "", "","",10,0, manager.OnMatchJoined);
                                }
                            }
                        }
                    }

                    /**
                    if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Change MM server"))
                    {
                        showServer = !showServer;
                    }
                    if (showServer)
                    {
                        ypos += spacing;
                        if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Local"))
                        {
                            manager.SetMatchHost("localhost", 1337, false);
                            showServer = false;
                        }
                        ypos += spacing;
                        if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Internet"))
                        {
                            manager.SetMatchHost("mm.unet.unity3d.com", 443, true);
                            showServer = false;
                        }
                        ypos += spacing;
                        if (GUI.Button(new Rect(xpos, ypos, 100, 20), "Staging"))
                        {
                            manager.SetMatchHost("staging-mm.unet.unity3d.com", 443, true);
                            showServer = false;
                        }
                    }
    
                    **/
                    

                    GUI.Label(new Rect(160, 160, 300, 20), "MM Uri: " + manager.matchMaker.baseUri);

                    /**
                    if (GUI.Button(new Rect(xpos, ypos, 200, 20), "Disable Match Maker"))
                    {
                        manager.StopMatchMaker();
                    }
                    **/
                }
            }
        }
    }
};
