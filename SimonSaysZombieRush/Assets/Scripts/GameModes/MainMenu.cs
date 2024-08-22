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

    void Start()
    {
        if (buildIndexesForLevels.Length > 0)
        {
            UpdateHighScore(timeTrial1Text);
        }
        if (buildIndexesForLevels.Length > 1)
        {
            UpdateHighScore(timeTrial2Text);
        }
        if (buildIndexesForLevels.Length > 2)
        {
            UpdateHighScore(timeTrial3Text);
        }
    }

    private void UpdateHighScore(TMP_Text text)
    {
        float bestTime = PlayerPrefs.GetFloat(buildIndexesForLevels[0].ToString());
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
}
