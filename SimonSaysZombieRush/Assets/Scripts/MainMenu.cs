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
    TimeTrialStats timeTrial1Stats;
    TimeTrialStats timeTrial2Stats;
    TimeTrialStats timeTrial3Stats;
    HordeStats horde1Stats;
    HordeStats horde2Stats;
    HordeStats horde3Stats;

    [SerializeField] TMP_Text timeTrial1Text;
    [SerializeField] TMP_Text timeTrial2Text;
    [SerializeField] TMP_Text timeTrial3Text;
    [SerializeField] TMP_Text horde1Text;
    [SerializeField] TMP_Text horde2Text;
    [SerializeField] TMP_Text horde3Text;

    void Start()
    {
        // TODO: Replace with a more programatic system without "magic" strings
        timeTrial1Stats = AssetDatabase.LoadAssetAtPath<TimeTrialStats>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("TimeTrialModeLevel1")[0]));
        timeTrial2Stats = AssetDatabase.LoadAssetAtPath<TimeTrialStats>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("TimeTrialModeLevel2")[0]));
        timeTrial3Stats = AssetDatabase.LoadAssetAtPath<TimeTrialStats>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("TimeTrialModeLevel3")[0]));
        horde1Stats = AssetDatabase.LoadAssetAtPath<HordeStats>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("HordeModeLevel1")[0]));
        horde2Stats = AssetDatabase.LoadAssetAtPath<HordeStats>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("HordeModeLevel2")[0]));
        horde3Stats = AssetDatabase.LoadAssetAtPath<HordeStats>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("HordeModeLevel3")[0]));

        if (timeTrial1Stats != null)
        {
            timeTrial1Text.text = timeTrial1Stats.BestTime.ToString("F0");
        }
        if (timeTrial2Stats != null)
        {
            timeTrial2Text.text = timeTrial2Stats.BestTime.ToString("F0");
        }
        if (timeTrial3Stats != null)
        {
            timeTrial3Text.text = timeTrial3Stats.BestTime.ToString("F0");
        }
        if (horde1Stats != null)
        {
            horde1Text.text = horde1Stats.HighestWave.ToString();
        }
        if (horde2Stats != null)
        {
            horde2Text.text = horde2Stats.HighestWave.ToString();
        }
        if (horde3Stats != null)
        {
            horde3Text.text = horde3Stats.HighestWave.ToString();
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
    TimeTrialStats GetTimeTrialStats(string statsName)
    {
        string[] assetGuids = AssetDatabase.FindAssets(statsName);
        if (assetGuids == null || assetGuids.Length <= 0)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
        return AssetDatabase.LoadAssetAtPath<TimeTrialStats>(path);
    }

    HordeStats GetHordeStats(string statsName)
    {
        string[] assetGuids = AssetDatabase.FindAssets(statsName);
        if (assetGuids == null || assetGuids.Length <= 0)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
        return AssetDatabase.LoadAssetAtPath<HordeStats>(path);
    }
}
