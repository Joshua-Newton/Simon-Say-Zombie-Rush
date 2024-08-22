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
            timeTrial1Text.text = PlayerPrefs.GetFloat(buildIndexesForLevels[0].ToString()).ToString("F0");
        }
        if (buildIndexesForLevels.Length > 1)
        {
            timeTrial2Text.text = PlayerPrefs.GetFloat(buildIndexesForLevels[1].ToString()).ToString("F0");
        }
        if (buildIndexesForLevels.Length > 2)
        {
            timeTrial3Text.text = PlayerPrefs.GetFloat(buildIndexesForLevels[2].ToString()).ToString("F0");
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
