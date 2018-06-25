using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {
    LivingEntity playerEntitity;

    public Image fadePlane;
    public GameObject gameOverUI;

    // Use this for initialization
    void Start ()
    {
        playerEntitity = FindObjectOfType<Player>();
        Action OnPlayerDeathAction = () => OnGameOver();
        playerEntitity.OnDeath += OnPlayerDeathAction;
    }

    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, Color.black, 1));
        gameOverUI.SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time){
        float speed = 1 / time;
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }


    // UI Input
    public void StartNewGame()
    {
        Application.LoadLevel("Main");
    }
}
