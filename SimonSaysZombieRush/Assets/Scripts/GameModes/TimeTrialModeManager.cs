using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimeTrialModeManager : GameManager
{
    public static new TimeTrialModeManager instance;

    [Header("Simon Says Mechanic")]
    [SerializeField] private int commandLength = 3; // Length of the command sequence
    [SerializeField] private TextMeshProUGUI commandDisplay; // TextMeshProUGUI to display the command
    [SerializeField] private GameObject[] commandImageObjects; // Possible positions for the images
    [SerializeField] private TextMeshProUGUI resultDisplay; // TextMeshProUGUI to display the result
    [SerializeField] private TextMeshProUGUI timerDisplay; // TextMeshProUGUI to display the timer
    [Header("Timers")]
    [SerializeField] private float levelTime = 120f; // Total time for the level in seconds
    [Header("Item Timers")]
    [SerializeField] private GameObject timerParent;
    [SerializeField] private GameObject itemTimerPrefab;
    [SerializeField] private float timerSpacing;
    [Header("Scoring")]
    [Range(0, 1000)] [SerializeField] int pointsPerItem = 100;
    [Range(0, 1000)] [SerializeField] int pointsPerKill = 25;
    [Range(0, 1000)] [SerializeField] int pointsBonusForSequence = 50;

    private float remainingTime;
    private List<GameObject> possibleItems; // List of possible items
    private List<GameObject> commandSequence; // The generated command sequence
    private List<GameObject> collectedSequence; // The player's collected sequence
    private List<GameObject> playerInventory; // All of the items the player is holding
    private List<GameObject> timers;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }

    void Start()
    {
        playerInventory = new List<GameObject>();
        collectedSequence = new List<GameObject>();
        timers = new List<GameObject>();
        InitializePossibleItems();
        InitializeTimerUI();
        GenerateCommand();
        DisplayCommand();
        DisplayImageCommand();
        StartLevelTimer();
    }

    void InitializePossibleItems()
    {
        possibleItems = GameObject.FindGameObjectsWithTag("Pickup").Distinct().ToList();
    }

    void InitializeTimerUI()
    {
        for(int i = 0; i < possibleItems.Count; ++i)
        {
            GameObject newTimer = Instantiate(itemTimerPrefab, timerParent.transform);
            RectTransform rt = newTimer.GetComponent<RectTransform>();
            if(rt != null )
            {
                rt.SetPositionAndRotation(new Vector3(rt.position.x, rt.position.y - (i * timerSpacing), rt.position.z), Quaternion.identity);
            }
            ItemTimer timerComponent = newTimer.GetComponent<ItemTimer>();
            if(timerComponent != null)
            {
                timerComponent.SetRemainingTime(possibleItems[i].GetComponent<ItemCollection>().GetSecondsToRetrieve());
                timerComponent.SetItem(possibleItems[i]);
                timerComponent.InitializeTimer(i);
                possibleItems[i].GetComponent<ItemCollection>().SetTimerIndex(i);
            }

            timers.Add(newTimer);
        }
    }

    public void EndTimer(int timerIndex)
    {
        timers[timerIndex].gameObject.SetActive(false);

        for (int i = timerIndex + 1; i < timers.Count; ++i)
        {
            RectTransform rt = timers[i].GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.SetPositionAndRotation(new Vector3(rt.position.x, rt.position.y + timerSpacing, rt.position.z), Quaternion.identity);
            }
        }

    }

    public void RemoveItemFromCollections(GameObject item)
    {
        if(possibleItems.Contains(item))
        {
            possibleItems.Remove(item);
        }
        if(playerInventory.Contains(item))
        {
            playerInventory.Remove(item);
        }
    }

    public void CheckSimonSequenceForUpdate()
    {
        for(int i = 0; i < commandSequence.Count; ++i)
        {
            if (!commandSequence[i].gameObject.activeSelf)
            {
                ResetGameSequence();
            }
        }
    }

    public void NotifyPlayerOfMissedItem(GameObject item)
    {
        StartCoroutine(ShowResult("Failed to collect " + item.name + " in time!"));
        
    }

    public void CheckForLossByMissingItems()
    {
        if (possibleItems.Count <= 0)
        {
            LoseGame();
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
            Resources.Load<GameObject>($"Prefabs/Essentials/SimonImg{possibleItems[randomIndex].name}"); // Load the image
        }
    }

    // Display the command sequence
    void DisplayCommand()
    {
        if(commandSequence.Count > 0)
        {
            commandDisplay.text = string.Join(", ", commandSequence.Select(item => item.name));
        }
        else
        {
            commandDisplay.text = "Return to base!";
        }

    }

    void DisplayImageCommand()
    {
        // If no command is left, disable the images and return
        if(commandSequence.Count <= 0)
        {
            for(int i = 0; i < commandImageObjects.Count(); ++i)
            {
                commandImageObjects[i].gameObject.SetActive(false);
            }
            return;
        }

        // There are commands remaining, so update the images
        for (int i = 0; i < commandSequence.Count && i < commandImageObjects.Length; ++i)
        {
            Image imgComponent = commandImageObjects[i].GetComponent<Image>();
            ItemCollection pickupComponent = commandSequence[i].GetComponent<ItemCollection>();
            if (imgComponent && pickupComponent)
            {
                imgComponent.sprite = pickupComponent.GetItemSprite();
                imgComponent.color = new Color(imgComponent.color.r, imgComponent.color.g, imgComponent.color.b, 1f);
            }
        }
    }

    // Display the timer in MM:SS format
    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerDisplay.text = $"{minutes:0}:{seconds:00}";
    }

    // Start the level timer
    void StartLevelTimer()
    {
        remainingTime = levelTime;
        StartCoroutine(LevelTimerCoroutine());
    }

    public void ReturnToBase()
    {
        UpdateScore(playerInventory.Count * pointsPerItem);
        
        if(possibleItems.Count <= 0)
        {
            StartCoroutine(ShowResultAndWin("All items collected"));
        }
        else
        {
            StartCoroutine(ShowResult("Collected " + playerInventory.Count.ToString() + " items!"));
        }

        for(int i = 0; i < playerInventory.Count; ++i)
        {
            EndTimer(playerInventory[i].GetComponent<ItemCollection>().GetTimerIndex());
        }

        playerInventory.Clear();
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
        playerInventory.Add(item);
        collectedSequence.Add(item);
        possibleItems.Remove(item); // Remove the item from the possibleItems list

        CheckCollectedSequence();
    }

    // Validate the player's collected sequence
    void CheckCollectedSequence()
    {
        for (int i = 0; i < collectedSequence.Count; i++)
        {
            if (collectedSequence[i] != commandSequence[i])
            {
                StartCoroutine(ShowResult("Incorrect sequence!"));
                ResetGameSequence();
                return;
            }
        }

        if (collectedSequence.Count == commandSequence.Count)
        {
            StartCoroutine(ShowResult("Correct sequence!"));
            ResetGameSequence();
        }
    }

    // Reset the game for a new command
    void ResetGameSequence()
    {
        collectedSequence.Clear();
        GenerateCommand();
        DisplayCommand();
        DisplayImageCommand();
    }

    public void UpdateScoreFromKill()
    {
        UpdateScore(pointsPerKill);
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
        menuActive = lastLevel ? menuWinLastLevel : menuWin;
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
            #if UNITY_EDITOR
                AssetDatabase.CreateAsset(newStats, savePath);
            #endif
        }
    }

    TimeTrialStats GetStats()
    {
        #if UNITY_EDITOR
            string[] assetGuids = AssetDatabase.FindAssets(statsAssetName);
            if (assetGuids == null || assetGuids.Length <= 0)
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
            return AssetDatabase.LoadAssetAtPath<TimeTrialStats>(path);
        #else
            return null;
        #endif
    }

    public override void UpdateStats(LevelStats stats)
    {
        if (stats is TimeTrialStats timeTrialStats)
        {
            timeTrialStats.BestTime = remainingTime;
            timeTrialStats.BestScore = score;
        }
        stats.EnemiesKilled = 0; // TODO: Implement enemies killed tracker and assign here
        stats.TimeUsed = 0; // TODO: Implement time tracker
    }
}
