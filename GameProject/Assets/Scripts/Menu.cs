using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;
    public GameObject InventoryMenuHolder;
    public GameObject playerStatsMenuHolder;

    public GameObject PlayerUI;

    public Slider[] volumeSliders;
    public Toggle[] resolutionToggles;
    public Toggle fullscreenToggle;
    public int[] screenWidths;

    int activeScreenResIndex;
    bool isFullscreenSave;

    private void Start()
    {
        activeScreenResIndex = PlayerPrefs.GetInt("screen res index");
        isFullscreenSave = (PlayerPrefs.GetInt("fullscreen") == 0);

        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].isOn = i == activeScreenResIndex;
        }
        
        SetFullscreen(isFullscreenSave);
    }

    public void Play(int i)
    {
        PlayerPrefs.SetInt("missionChoose", i);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Main");
    }

    public void BuildMap()
    {
        SceneManager.LoadScene("BuildMap");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OptionsMenu()
    {
        mainMenuHolder.SetActive(false);
        InventoryMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(true);
        PlayerUI.SetActive(false);
        playerStatsMenuHolder.SetActive(false);
    }

    public void MainMenu()
    {
        mainMenuHolder.SetActive(true);
        InventoryMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(false);
        PlayerUI.SetActive(false);
        playerStatsMenuHolder.SetActive(false);
    }

    public void InventoryMenu()
    {
        mainMenuHolder.SetActive(false);
        InventoryMenuHolder.SetActive(true);
        optionsMenuHolder.SetActive(false);
        PlayerUI.SetActive(true);
        playerStatsMenuHolder.SetActive(false);
    }

    public void PlayerStatsMenu()
    {
        mainMenuHolder.SetActive(false);
        InventoryMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(false);
        PlayerUI.SetActive(true);
        playerStatsMenuHolder.SetActive(true);
    }

    public void SetScreenResolution(int i)
    {
        if (resolutionToggles[i].isOn)
        {
            activeScreenResIndex = i;
            float aspectRatio = 16 / 9f;
            Screen.SetResolution(screenWidths[i], (int) (screenWidths[i] / aspectRatio), false);
            PlayerPrefs.SetInt("screen res index", activeScreenResIndex);
            PlayerPrefs.Save();
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        //isFullscreenSave = !isFullscreenSave;
        if (isFullscreen == isFullscreenSave)
            isFullscreen = !isFullscreen;

        fullscreenToggle.isOn = isFullscreen;
        for (int i = 0; i < resolutionToggles.Length; i++)
        {
            resolutionToggles[i].interactable = !isFullscreen;
        }
        

        if(isFullscreen)
        {
            Resolution[] allResolutions = Screen.resolutions;
            Resolution maxResolution = allResolutions[allResolutions.Length - 1];
            Screen.SetResolution(maxResolution.width, maxResolution.height, true);
        } else
        {
            SetScreenResolution(activeScreenResIndex);
        }

        PlayerPrefs.SetInt("fullscreen", ((isFullscreen) ? 1 : 0));
        PlayerPrefs.Save();

        isFullscreenSave = isFullscreen;
    }

    public void SetMasterVolume(float value)
    {
        // AudioManager.instance.SetVolume(value, AudioManager.AudionChannel.Master);
    }

    public void SetMusicVolume(float value)
    {
        // AudioManager.instance.SetVolume(value, AudioManager.AudionChannel.Music);
    }


    public void SetSfxVolume(float value)
    {
        // AudioManager.instance.SetVolume(value, AudioManager.AudionChannel.Sfx);
    }    
}
