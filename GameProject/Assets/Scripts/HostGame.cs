using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class HostGame : MonoBehaviour {

    private uint roomSize = 4;

    private string roomName;

    private NetworkManager networkManager;


    public InputField nameInputField;

    void Start()
    {
        networkManager = NetworkManager.singleton;
        if (networkManager.matchMaker == null)
            networkManager.StartMatchMaker();
    }

    public void SetRoomName()
    {
        this.roomName = nameInputField.text;
    }

    public void CreateRoom()
    {
        if(roomName != null && roomName != "")
        {
            Debug.Log("Creating Room: " + roomName + " with room for " + roomSize + " players.");
            // Create room
            networkManager.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0, networkManager.OnMatchCreate);
        }
    }
}
