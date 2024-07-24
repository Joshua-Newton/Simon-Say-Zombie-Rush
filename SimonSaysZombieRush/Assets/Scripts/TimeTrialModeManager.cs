using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimeTrialModeManager : GameManager
{
    public static new TimeTrialModeManager instance;

    [SerializeField] protected int commandLength = 3; // Length of the command sequence
    [SerializeField] protected TextMeshProUGUI commandDisplay; // TextMeshProUGUI to display the command
    [SerializeField] protected GameObject[] commandImageObjects; // Possible positions for the images
    [SerializeField] protected TextMeshProUGUI resultDisplay; // TextMeshProUGUI to display the result
    [SerializeField] protected TextMeshProUGUI timerDisplay; // TextMeshProUGUI to display the timer
    [SerializeField] protected float levelTime = 60f; // Total time for the level in seconds

    private float remainingTime;

    protected List<GameObject> possibleItems; // List of possible items
    protected List<GameObject> commandSequence; // The generated command sequence
    protected List<string> playerSequence; // The player's collected sequence

    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }

    void Start()
    {
        InitializePossibleItems();
        GenerateCommand();
        DisplayCommand();
        DisplayImageCommand();
        StartLevelTimer();
    }

    void InitializePossibleItems()
    {
        possibleItems = new List<GameObject>();
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (GameObject pickup in pickups)
        {
            if (!possibleItems.Contains(pickup))
            {
                possibleItems.Add(pickup);
            }
        }
    }

    // Generate a random command sequence
    void GenerateCommand()
    {
        commandSequence = new List<GameObject>();
        for (int i = 0; i < commandLength; i++)
        {
            if (possibleItems.Count == 0) break; // No more items to add

            int randomIndex = Random.Range(0, possibleItems.Count);
            commandSequence.Add(possibleItems[randomIndex]);
            GameObject itemName = Resources.Load<GameObject>("Prefabs/Essentials/SimonImg" + possibleItems[randomIndex]); // Display Simons image
        }
    }

    // Display the command sequence
    void DisplayCommand()
    {
        var commandNames = commandSequence.Select(item => item.name);
        commandDisplay.text = string.Join(", ", commandNames);
    }

    void DisplayImageCommand()
    {
        for (int i = 0; i < commandSequence.Count && i < commandImageObjects.Length; ++i)
        {
            Image imgComponent = commandImageObjects[i].GetComponent<Image>();
            ItemCollection pickupComponent = commandSequence[i].GetComponent<ItemCollection>();
            if (imgComponent && pickupComponent)
            {
                imgComponent.sprite = pickupComponent.GetItemSprite();
                imgComponent.color = new Color(imgComponent.color.r, imgComponent.color.g, imgComponent.color.b, 255);
            }
        }
    }

    // Display the timer in MM:SS format
    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerDisplay.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    // Start the level timer
    void StartLevelTimer()
    {
        remainingTime = levelTime;
        StartCoroutine(LevelTimerCoroutine());
    }

    // Coroutine to manage the level timer
    IEnumerator LevelTimerCoroutine()
    {
        while (remainingTime > 0)
        {
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
            UpdateTimerDisplay();
        }

        // Time is up, player loses the game
        remainingTime = 0;
        UpdateTimerDisplay();
        LoseGame();
    }

    // Call this function when the player collects an item
    public override void CollectItem(GameObject item)
    {
        if (playerSequence == null)
        {
            playerSequence = new List<string>();
        }

        playerSequence.Add(item.name);
        possibleItems.Remove(item); // Remove the item from the possibleItems list

        CheckPlayerSequence();
    }

    // Validate the player's collected sequence
    void CheckPlayerSequence()
    {
        // Check if the player's collected sequence matches the command sequence so far
        for (int i = 0; i < playerSequence.Count; i++)
        {
            if (playerSequence[i] != commandSequence[i].name)
            {
                StartCoroutine(ShowResult("Incorrect sequence!"));
                ResetGameSequence();
                return;
            }
        }

        if (possibleItems.Count == 0)
        {
            StartCoroutine(ShowResultAndWin("All items collected"));
            UpdateScore(100);
        }
        else if (playerSequence.Count == commandSequence.Count)
        {
            // Keep the image if the sequence is correct but not all items are collected yet
            StartCoroutine(ShowResult("Correct sequence!"));
            UpdateScore(100); // Update score for correct sequence
            ResetGameSequence();
        }
    }

    // Reset the game for a new command
    void ResetGameSequence()
    {
        playerSequence.Clear();
        GenerateCommand();
        DisplayCommand();
        DisplayImageCommand();
    }

    // Update the score and display it
    void UpdateScore(int points)
    {
        score += points;
        scoreText.text = "Score: " + score;
        scoreTextFinalLevel.text = "Score: " + score;
    }

    // Coroutine to show the result for 2 seconds
    IEnumerator ShowResult(string message)
    {
        resultDisplay.text = message;
        resultDisplay.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        resultDisplay.gameObject.SetActive(false);
    }

    // Coroutine to show the result and then display the win menu
    IEnumerator ShowResultAndWin(string message)
    {
        resultDisplay.text = message;
        resultDisplay.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        resultDisplay.gameObject.SetActive(false);
        WinGame();
        StartCoroutine(LoadNextLevel());
    }

    // Coroutine to load the next level
    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(2); // Optional delay before loading the next scene
        SceneManager.LoadScene(SceneManager.GetSceneByPath(nextScenePath).name);
    }

    public override void LoseGame()
    {
        menuActive = menuLose;
        PauseAndOpenActiveMenu();
    }

    public override void WinGame()
    {
        if (lastLevel)
        {
            menuActive = menuWinLastLevel;
        }
        else
        {
            menuActive = menuWin;
        }
        PauseAndOpenActiveMenu();
        SaveStats();
    }

    void SaveStats()
    {
        TimeTrialStats currentStats = GetStats();
        if (currentStats != null)
        {
            if (remainingTime > currentStats.TimeUsed)
            {
                UpdateStats(currentStats);
            }
        }
        else
        {
            TimeTrialStats newStats = ScriptableObject.CreateInstance<TimeTrialStats>();
            UpdateStats(newStats);
            AssetDatabase.CreateAsset(newStats, savePath);
        }
    }

    TimeTrialStats GetStats()
    {
        string[] assetGuids = AssetDatabase.FindAssets(statsAssetName);
        if (assetGuids == null || assetGuids.Length <= 0)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
        return AssetDatabase.LoadAssetAtPath<TimeTrialStats>(path);
    }

    public override void UpdateStats(LevelStats stats)
    {
        if (stats.GetType() == typeof(TimeTrialStats))
        {
            ((TimeTrialStats)stats).BestTime = remainingTime;
            ((TimeTrialStats)stats).BestScore = score;
        }
        stats.EnemiesKilled = 0; // TODO: Implement enemies killed tracker and assign here
        stats.TimeUsed = 0; // TODO: Implement time tracker    }
    }
}
