using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{

    public Button[] buttons;

    /*
    private void Awake()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 0);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = true;
        }
    }
    */

    public void OpenLevel (int levelID)
    {
        string TrialLevel = "TimeTrialModeLevel" + levelID;
        SceneManager.LoadScene(TrialLevel);
        string HoardLevel = "HordeModeLevel" + levelID;
        SceneManager.LoadScene(HoardLevel);


    }
}
