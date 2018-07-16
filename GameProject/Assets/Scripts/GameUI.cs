using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class GameUI : NetworkBehaviour
{
    public Player playerEntitity;

    public Image fadePlane;
    public GameObject gameOverUI;

    public RectTransform newWaveBanner;
    public RectTransform bulletsUI;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;
    public Text scoreUI;
    public Text streakUI;
    public Text bulletsInMag;
    public RectTransform healthBar;

    public Text accuracyText;
    public RectTransform accuracyUI;

    public Text gameOverScoreUI;

    public RectTransform fireModeChange;
    public Text fireModeTxt;
    public GunController currentGunController;
    public Gun currentGun;
    // Use this for initialization

    // Banners
    float delayTime = 1.5f;
    float speed = 2.5f;
    float endDelayTime;
    bool isChangingFireMode = false;
    bool gameOver = false;
    public ScoreKeeper scoreKeeper;
    
    public override void OnStartServer()
    {
        //Action OnPlayerDeathAction = () => OnGameOver();
        //playerEntitity.OnDeath += OnPlayerDeathAction;
    }

    void Awake()
    {

    }

    private void Update()
    {
        if(playerEntitity != null) {
        if (scoreKeeper == null)
            scoreKeeper = FindObjectOfType<ScoreKeeper>();
        
        if (!gameOver) {

        scoreUI.text = scoreKeeper.score.ToString("D6");

        if (ScoreKeeper.streakCount == 0)
            streakUI.text = "";
        else
            streakUI.text = "x" + (ScoreKeeper.streakCount + 1);
        
        currentGun = currentGunController.equippedGun;

        if (currentGun != null)
           bulletsInMag.text = currentGun.projectilesRemainingInMag + "/" + currentGun.projectilesPerMag;

        accuracyText.text = ((currentGunController.accuracy) * 100).ToString("n2") + "%";

        float healthPercent = 0;
        if(playerEntitity != null) { 
            healthPercent = playerEntitity.health / playerEntitity.startingHealth;
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);
        }
        }
    }
    
    IEnumerator AnimateNewFireMode()
    {
        isChangingFireMode = true;
        float animatePercent = 0;
        int dir = 1;
        endDelayTime = Time.time + 1 / speed + delayTime;
        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1)
            {
                animatePercent = 1;
                if (Time.time > endDelayTime) {
                    dir = -1;
                    isChangingFireMode = false;
                }
            }
            fireModeChange.anchoredPosition = Vector2.up * Mathf.Lerp(-160, 0, animatePercent);
            yield return null;
        }
    }

    IEnumerator AnimateNewWaveBanner()
    {
        float animatePercent = 0;
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1)
            {
                animatePercent = 1;
                if (Time.time > endDelayTime)
                    dir = -1;
            }
            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-160, 0, animatePercent);
            yield return null;
        }
    }

    public void OnNewFireMode(string fireMode)
    {
        fireModeTxt.text = "Fire mode: " + fireMode;
        if(isChangingFireMode)
        {
            endDelayTime = (Time.time + 1 / 2.5f) + 1.5f;
        } else {
            endDelayTime = 1.5f;
            StopCoroutine("AnimateNewFireMode");
            StartCoroutine("AnimateNewFireMode");
        }
    }

    public void OnNewWave(int waveNumber, int enemies)
    {
        string[] numbers = { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve" };
        if(waveNumber > numbers.Length)
        {
            newWaveTitle.text = "- Wave Infinite -";
        } else
            newWaveTitle.text = "- Wave " + numbers[waveNumber - 1] + " - ";

        String enemyCountString;

        enemyCountString = enemies + "";
        if (enemies == -1)
        {
            enemyCountString = "Infinite";
        }
        newWaveEnemyCount.text = "Enemies: " + enemyCountString;
        
        CmdStartNewWave(newWaveTitle.text, newWaveEnemyCount.text);
    }

    [Command]
    public void CmdStartNewWave(String waveName, String enemiesCount)
    {
        RpcStartNewWave(waveName, enemiesCount);
    }

    [ClientRpc]
    public void RpcStartNewWave(String waveName, String enemiesCount)
    {
        StopCoroutine("AnimateNewWaveBanner");
        newWaveTitle.text = waveName;
        newWaveEnemyCount.text = enemiesCount;
        StartCoroutine("AnimateNewWaveBanner");
    }

    void OnGameOver()
    {
        gameOver = true;
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear, new Color(0,0,0,.90f), 1));
        gameOverScoreUI.text = scoreUI.text;
        scoreUI.gameObject.SetActive(false);
        bulletsUI.gameObject.SetActive(false);
        accuracyUI.gameObject.SetActive(false);
        healthBar.transform.parent.gameObject.SetActive(false);
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
        SceneManager.LoadScene("Main");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");

    }
}
