using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatesMenu : MonoBehaviour {
    public Text lifeText;
    public Text luckyText;
    public Text speedText;

    // Use this for initialization
    void Start () {
        PlayerPrefs.SetInt("PlayerLifeStart", 20);
        PlayerPrefs.SetInt("PlayerLucky", 3);
        PlayerPrefs.SetFloat("PlayerSpeed", 5);
        updateText();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void updateText()
    {
        lifeText.text = PlayerPrefs.GetInt("PlayerLifeStart", 20) + "";
        luckyText.text = PlayerPrefs.GetInt("PlayerLucky", 3) + "%";
        speedText.text = PlayerPrefs.GetFloat("PlayerSpeed", 1) + " km/h" ;
    }
}
