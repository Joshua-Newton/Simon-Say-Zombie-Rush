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

    [Header("----- Simon Says Mechanic -----")]
    [SerializeField] private int commandLength = 3;
    [SerializeField] private TextMeshProUGUI commandDisplay;
    [SerializeField] private GameObject[] commandImageObjects;
    [SerializeField] private TextMeshProUGUI resultDisplay;
    [SerializeField] private TextMeshProUGUI timerDisplay;

    [Header("----- Item Timers -----")]
    [SerializeField] private GameObject timerParent;
    [SerializeField] private GameObject itemTimerPrefab;
    [SerializeField] private float timerSpacing;

    [Header("----- Scoring -----")]
    [Range(0, 1000)][SerializeField] int pointsPerItem = 100;
    [Range(0, 1000)][SerializeField] int pointsPerKill = 25;
    [Range(0, 1000)][SerializeField] int pointsBonusForSequence = 50;
    [Range(1, 6000)][SerializeField] int parTimeSeconds = 60;

    [Header("----- Grenade UI -----")]
    [SerializeField] private Image grenadeIcon;
    [SerializeField] private TMP_Text grenadeCountText;
    [SerializeField] private List<Sprite> grenadeSprites;  // List of sprites corresponding to each grenade type
    private Player pj;

    private int currentGrenades;
    private float timePassed;
    private List<GameObject> possibleItems;
    private List<GameObject> commandSequence;
    private List<GameObject> collectedSequence;
    private List<GameObject> playerInventory;
    private List<GameObject> timers;
    private int totalCollectedItems = 0;
    private GameObject baseReturnZone;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }

    protected override void Start()
    {
        base.Start();
        baseReturnZone = FindObjectOfType<BaseReturnZone>().gameObject;
        playerInventory = new List<GameObject>();
        collectedSequence = new List<GameObject>();
        timers = new List<GameObject>();
        pj = GameManager.instance.player.GetComponent<Player>();
        InitializePossibleItems();
        InitializeTimerUI();
        ResetTimer();
        GenerateCommand();
        DisplayCommand();
        DisplayImageCommand();
        UpdateGrenadeUI();
    }

    private void UpdateGrenadeUI()
    {
        if (pj != null)
        {
            GameObject selectedGrenade = pj.GetSelectedGrenade();
            if (selectedGrenade != null)
            {
                int currentGrenades = pj.GetCurrentGrenades();
                int maxGrenades = pj.GetMaxGrenades(selectedGrenade);

                grenadeCountText.text = $"{currentGrenades}/{maxGrenades}";
                grenadeIcon.fillAmount = (float)currentGrenades / maxGrenades;

                // Update the grenade icon sprite based on the selected grenade type
                int grenadeIndex = pj.GetSelectedGrenadeIndex();  // Assuming this method exists to get the index of the selected grenade
                if (grenadeIndex >= 0 && grenadeIndex < grenadeSprites.Count)
                {
                    grenadeIcon.sprite = grenadeSprites[grenadeIndex];
                }

            }
            else
            {
                grenadeCountText.text = "0/0";
                grenadeIcon.fillAmount = 0;
                grenadeIcon.sprite = null;  // Clear the icon if no grenade is selected
            }
        }
    }


    void InitializePossibleItems()
    {
        possibleItems = GameObject.FindGameObjectsWithTag("Pickup").Distinct().ToList();
    }

    void InitializeTimerUI()
    {
        possibleItems.Sort((a, b) => a.GetComponent<ItemCollection>().GetSecondsToRetrieve().CompareTo(b.GetComponent<ItemCollection>().GetSecondsToRetrieve()));

        for (int i = 0; i < possibleItems.Count; ++i)
        {
            GameObject newTimer = Instantiate(itemTimerPrefab, timerParent.transform);
            RectTransform rt = newTimer.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.SetPositionAndRotation(new Vector3(rt.position.x, rt.position.y - (i * timerSpacing), rt.position.z), Quaternion.identity);
            }
            ItemTimer timerComponent = newTimer.GetComponent<ItemTimer>();
            if (timerComponent != null)
            {
                timerComponent.SetRemainingTime(possibleItems[i].GetComponent<ItemCollection>().GetSecondsToRetrieve());
                timerComponent.SetItem(possibleItems[i]);
                timerComponent.InitializeTimer(i);
                possibleItems[i].GetComponent<ItemCollection>().SetTimerIndex(i);
            }

            timers.Add(newTimer);
        }
    }

    void GenerateCommand()
    {
        commandSequence = new List<GameObject>();
        for (int i = 0; i < commandLength; i++)
        {
            if (possibleItems.Count == 0) break;
            int nextItemIndex = GetLowestTimeItemIndex(possibleItems);
            commandSequence.Add(possibleItems[nextItemIndex]);
            Resources.Load<GameObject>($"Prefabs/Essentials/SimonImg{possibleItems[nextItemIndex].name}");
        }
    }

    int GetLowestTimeItemIndex(List<GameObject> objectives)
    {
        int min = int.MaxValue;
        int minIndex = -1;
        for (int i = 0; i < objectives.Count(); i++)
        {
            GameObject objective = objectives[i];
            int seconds = objective.GetComponent<ItemCollection>().GetSecondsToRetrieve();
            if (seconds < min)
            {
                min = Mathf.Min(min, seconds);
                minIndex = i;
            }
        }
        return minIndex;
    }

    void DisplayCommand()
    {
        if (commandSequence.Count > 0)
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
        if (commandSequence.Count <= 0)
        {
            for (int i = 0; i < commandImageObjects.Length; ++i)
            {
                commandImageObjects[i].gameObject.SetActive(false);
            }
            return;
        }

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

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timePassed / 60);
        int seconds = Mathf.FloorToInt(timePassed % 60);
        timerDisplay.text = $"{minutes:0}:{seconds:00}";
    }

    protected override void Update()
    {
        base.Update();
        if (!isPaused)
        {
            timePassed += Time.deltaTime;
            UpdateTimerDisplay();
        }
        UpdateGrenadeUI();
    }

    public void ResetTimer()
    {
        timePassed = 0f;
        UpdateTimerDisplay();
    }

    public void ReturnToBase()
    {
        UpdateScore(playerInventory.Count * pointsPerItem);
        totalCollectedItems += playerInventory.Count;

        if (possibleItems.Count <= 0 && playerInventory.Count <= 0)
        {
            StartCoroutine(ShowResultAndWin("Returned To Base Successfully"));
        }
        else if (possibleItems.Count <= 0)
        {
            StartCoroutine(ShowResultAndWin("All possible items collected"));
        }
        else
        {
            StartCoroutine(ShowResult("Collected " + playerInventory.Count.ToString() + " items!"));
        }

        for (int i = 0; i < playerInventory.Count; ++i)
        {
            EndTimer(playerInventory[i].GetComponent<ItemCollection>().GetTimerIndex());
        }

        playerInventory.Clear();
    }

    public override void CollectItem(GameObject item)
    {
        playerInventory.Add(item);
        collectedSequence.Add(item);
        possibleItems.Remove(item);
        playerScript.UpdateTargetObjective();
        CheckCollectedSequence();
    }

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
            StartCoroutine(ShowResult("Correct sequence! Bonus points awarded!"));
            UpdateScore(pointsBonusForSequence);
            ResetGameSequence();
        }
    }

    void ResetGameSequence()
    {
        collectedSequence.Clear();
        GenerateCommand();
        DisplayCommand();
        DisplayImageCommand();
    }

    public void HandleExpiredItem(GameObject item, int timerIndex)
    {
        EndTimer(timerIndex);
        RemoveItemFromCollections(item);
        NotifyPlayerOfMissedItem(item);
        CheckSimonSequenceForUpdate();
        CheckForLossByMissingItems();
        playerScript.UpdateTargetObjective();
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
        if (possibleItems.Contains(item))
        {
            possibleItems.Remove(item);
        }
        if (playerInventory.Contains(item))
        {
            playerInventory.Remove(item);
        }
    }

    public void CheckSimonSequenceForUpdate()
    {
        for (int i = 0; i < commandSequence.Count; ++i)
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
        if (possibleItems.Count <= 0 && totalCollectedItems <= 0)
        {
            LoseGame("You couldn't collect any resources!");
        }
    }

    public void UpdateScoreFromKill()
    {
        UpdateScore(pointsPerKill);
    }

    void UpdateScore(int points)
    {
        score += points;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
        scoreTextFinalLevel.text = "Score: " + score;
    }

    IEnumerator ShowResult(string message)
    {
        resultDisplay.text = message;
        resultDisplay.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        resultDisplay.gameObject.SetActive(false);
    }

    IEnumerator ShowResultAndWin(string message)
    {
        resultDisplay.text = message;
        resultDisplay.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        resultDisplay.gameObject.SetActive(false);
        WinGame();
        StartCoroutine(LoadNextLevel());
    }

    IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(SceneManager.GetSceneByPath(nextScenePath).name);
    }

    public override void LoseGame(string reason)
    {
        loseMessage.text = reason;
        menuActive = menuLose;
        PauseAndOpenActiveMenu();
    }

    public override void WinGame()
    {
        menuActive = (lastLevel || isStandAloneLevel) ? menuWinLastLevel : menuWin;
        DetermineFinalScore();
        UpdateScoreText();
        PauseAndOpenActiveMenu();
        SaveStats();
    }

    void DetermineFinalScore()
    {
        float scoreMultiplier = parTimeSeconds / timePassed;
        score = (int)(score * scoreMultiplier);
    }

    void SaveStats()
    {
        string statKey = SceneManager.GetActiveScene().buildIndex.ToString();
        float bestTime = PlayerPrefs.GetFloat(statKey, float.MaxValue);
        if(timePassed < bestTime)
        {
            PlayerPrefs.SetFloat(statKey, timePassed);
        }
    }

    public GameObject GetNextActiveObjective()
    {
        if(possibleItems.Count > 0)
        {
            possibleItems.Sort((a, b) => a.GetComponent<ItemCollection>().GetSecondsToRetrieve().CompareTo(b.GetComponent<ItemCollection>().GetSecondsToRetrieve()));
            return possibleItems[0];
        }
        return null;
    }

    public GameObject GetBaseReturnZone()
    {
        return baseReturnZone;
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
            timeTrialStats.BestTime = timePassed;
            timeTrialStats.BestScore = score;
        }
        stats.EnemiesKilled = 0;
        stats.TimeUsed = 0;
    }
}
