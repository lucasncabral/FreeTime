using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking.Match;

public class RoomListIten : MonoBehaviour {
    [SerializeField]
    private Text roomNameText;

    public delegate void JoinRoomDelegate(MatchInfoSnapshot match);
    JoinRoomDelegate joinRoomCallback;

    private MatchInfoSnapshot match;

    public void Setup(MatchInfoSnapshot match, JoinRoomDelegate joinRoomCallback)
    {
        this.match = match;

        this.joinRoomCallback = joinRoomCallback; 
        roomNameText.text = match.name + " (" + match.currentSize + "/" + match.maxSize + ")";
    }

    public void JoinRoom()
    {
        joinRoomCallback.Invoke(match);
    }
}
