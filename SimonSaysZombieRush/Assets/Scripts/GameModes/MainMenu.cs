using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour
{
    [SerializeField] TMP_Text timeTrial1Text;
    [SerializeField] TMP_Text timeTrial2Text;
    [SerializeField] TMP_Text timeTrial3Text;
    [SerializeField] int[] buildIndexesForLevels;
    [SerializeField] SettingsMenuManager settingsMenuManager;

    void Start()
    {
        InitializeSound();

        if (buildIndexesForLevels.Length > 0)
        {
            UpdateHighScore(timeTrial1Text, 0);
        }
        if (buildIndexesForLevels.Length > 1)
        {
            UpdateHighScore(timeTrial2Text, 1);
        }
        if (buildIndexesForLevels.Length > 2)
        {
            UpdateHighScore(timeTrial3Text, 2);
        }
    }

    private void UpdateHighScore(TMP_Text text, int index)
    {
        float bestTime = PlayerPrefs.GetFloat(buildIndexesForLevels[index].ToString());
        if (bestTime > 0)
        {
            text.text = bestTime.ToString("F0");
        }
        else
        {
            text.text = "--";
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void OpenLevel(int levelIndex)
    {
        SceneManager.LoadSceneAsync(levelIndex);
    }

    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    private void InitializeSound()
    {
        float cachedMasterVol = PlayerPrefs.GetFloat(SettingsMenuManager.MASTER_VOLUME_NAME, SettingsMenuManager.defaultMasterVol);
        float cachedMusicVol =  PlayerPrefs.GetFloat(SettingsMenuManager.MUSIC_VOLUME_NAME, SettingsMenuManager.defaultMusicVol);
        float cachedSFXVol =    PlayerPrefs.GetFloat(SettingsMenuManager.SFX_VOLUME_NAME, SettingsMenuManager.defaultSFXVol);
        float cachedMenuVol =   PlayerPrefs.GetFloat(SettingsMenuManager.MENU_VOLUME_NAME, SettingsMenuManager.defaultMenuVol);
        settingsMenuManager.mainAudioMixer.SetFloat(SettingsMenuManager.MASTER_VOLUME_NAME, SettingsMenuManager.SliderValToDecibels(cachedMasterVol));
        settingsMenuManager.mainAudioMixer.SetFloat(SettingsMenuManager.MUSIC_VOLUME_NAME, SettingsMenuManager.SliderValToDecibels(cachedMusicVol));
        settingsMenuManager.mainAudioMixer.SetFloat(SettingsMenuManager.SFX_VOLUME_NAME, SettingsMenuManager.SliderValToDecibels(cachedSFXVol));
        settingsMenuManager.mainAudioMixer.SetFloat(SettingsMenuManager.MENU_VOLUME_NAME, SettingsMenuManager.SliderValToDecibels(cachedMenuVol));
    }

}
