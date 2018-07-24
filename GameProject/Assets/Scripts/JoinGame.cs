using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using System;

public class JoinGame : MonoBehaviour {
    private NetworkManager networkManager;

    [SerializeField]
    private Text status;
    [SerializeField]
    private GameObject roomListItemPrefab;
    [SerializeField]
    private Transform roomListParent;

    List<GameObject> roomList = new List<GameObject>();

    void Start()
    {
        networkManager = NetworkManager.singleton;
        if (networkManager.matchMaker == null)
            networkManager.StartMatchMaker();
        RefreshRoomList();
    }

    public void RefreshRoomList()
    {
        ClearRoomList();
        networkManager.matchMaker.ListMatches(0, 20, "", false, 0, 0, OnMatchList);
        status.text = "Loading...";
    }

    private void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        status.text = "";
        if (!success && matchList == null) {
            status.text = "Couldn't get room list.";
            return;
        }
        ClearRoomList();

        foreach(MatchInfoSnapshot a in matchList)
        {
            GameObject roomListItemGO = Instantiate(roomListItemPrefab);
            roomListItemGO.transform.SetParent(roomListParent);
            roomListItemGO.transform.localScale = new Vector3(1, 1, 1);

            RoomListIten roomListItem = roomListItemGO.GetComponent<RoomListIten>();
            if (roomListItem != null)
                roomListItem.Setup(a, JoinRoom);

            roomList.Add(roomListItemGO);
        }

        if (roomList.Count == 0)
            status.text = "No rooms at the moment.";
    }

    public void JoinRoom(MatchInfoSnapshot match)
    {
        networkManager.matchMaker.JoinMatch(match.networkId, "", "","",0,0, networkManager.OnMatchJoined);
        ClearRoomList();
        status.text = "JOINING...";
    }

    void ClearRoomList()
    {
        for(int i = 0; i < roomList.Count; i++)
        {
            Destroy(roomList[i]);
        }

        roomList.Clear();
    }
	
}
