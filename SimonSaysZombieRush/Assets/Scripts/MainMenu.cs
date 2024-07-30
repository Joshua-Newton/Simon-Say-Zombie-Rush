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
    [SerializeField] TimeTrialStats timeTrial1Stats;
    [SerializeField] TimeTrialStats timeTrial2Stats;
    [SerializeField] TimeTrialStats timeTrial3Stats;
    [SerializeField] HordeStats horde1Stats;
    [SerializeField] HordeStats horde2Stats;
    [SerializeField] HordeStats horde3Stats;

    [SerializeField] TMP_Text timeTrial1Text;
    [SerializeField] TMP_Text timeTrial2Text;
    [SerializeField] TMP_Text timeTrial3Text;
    [SerializeField] TMP_Text horde1Text;
    [SerializeField] TMP_Text horde2Text;
    [SerializeField] TMP_Text horde3Text;

    void Start()
    {
        if (timeTrial1Stats != null)
        {
            timeTrial1Text.text = timeTrial1Stats.BestTime.ToString("F0");
            #if UNITY_EDITOR
                EditorUtility.SetDirty(timeTrial1Stats);
            #endif
        }
        if (timeTrial2Stats != null)
        {
            timeTrial2Text.text = timeTrial2Stats.BestTime.ToString("F0");
            #if UNITY_EDITOR
                EditorUtility.SetDirty(timeTrial2Stats);
            #endif
        }
        if (timeTrial3Stats != null)
        {
            timeTrial3Text.text = timeTrial3Stats.BestTime.ToString("F0");
            #if UNITY_EDITOR
                EditorUtility.SetDirty(timeTrial3Stats);
            #endif

        }
        if (horde1Stats != null)
        {
            horde1Text.text = horde1Stats.HighestWave.ToString();
            #if UNITY_EDITOR
                EditorUtility.SetDirty(horde1Stats);
            #endif
        }
        if (horde2Stats != null)
        {
            horde2Text.text = horde2Stats.HighestWave.ToString();
            #if UNITY_EDITOR
                EditorUtility.SetDirty(horde2Stats);
            #endif
        }
        if (horde3Stats != null)
        {
            horde3Text.text = horde3Stats.HighestWave.ToString();
            #if UNITY_EDITOR
                EditorUtility.SetDirty(horde3Stats);
            #endif
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
