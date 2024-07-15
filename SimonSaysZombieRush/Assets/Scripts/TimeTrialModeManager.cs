using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class TimeTrialModeManager : GameManager
{
    public static new TimeTrialModeManager instance;

    [SerializeField] protected int commandLength = 3; // Length of the command sequence
    [SerializeField] protected TextMeshProUGUI commandDisplay; // TextMeshProUGUI to display the command
    [SerializeField] protected TextMeshProUGUI resultDisplay; // TextMeshProUGUI to display the result

    protected List<string> possibleItems; // List of possible items
    protected List<string> commandSequence; // The generated command sequence
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
    }

    void InitializePossibleItems()
    {
        possibleItems = new List<string>();
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (GameObject pickup in pickups)
        {
            string itemName = pickup.name; // Use the name of the game object
            if (!possibleItems.Contains(itemName))
            {
                possibleItems.Add(itemName);
            }
        }
    }

    // Generate a random command sequence
    void GenerateCommand()
    {
        commandSequence = new List<string>();
        for (int i = 0; i < commandLength; i++)
        {
            if (possibleItems.Count == 0) break; // No more items to add

            int randomIndex = Random.Range(0, possibleItems.Count);
            commandSequence.Add(possibleItems[randomIndex]);
        }
    }

    // Display the command sequence
    void DisplayCommand()
    {
        commandDisplay.text = "Command: " + string.Join(", ", commandSequence);
    }

    // Call this function when the player collects an item
    public override void CollectItem(string item)
    {
        if (playerSequence == null)
        {
            playerSequence = new List<string>();
        }

        playerSequence.Add(item);
        possibleItems.Remove(item); // Remove the item from the possibleItems list
        CheckPlayerSequence();
    }

    // Validate the player's collected sequence
    void CheckPlayerSequence()
    {
        // Check if the player's collected sequence matches the command sequence so far
        for (int i = 0; i < playerSequence.Count; i++)
        {
            if (playerSequence[i] != commandSequence[i])
            {
                StartCoroutine(ShowResult("Incorrect sequence!"));
                ResetGame();
                return;
            }
        }

        // If all items are collected and the sequence is correct
        if (possibleItems.Count == 0)
        {
            StartCoroutine(ShowResultAndWin("All items collected"));
            UpdateScore(100);
        }
        else if (playerSequence.Count == commandSequence.Count)
        {
            // If the current sequence is correct but not all items are collected yet
            UpdateScore(100); // Update score for correct sequence
            StartCoroutine(ShowResult("Correct sequence!"));
            ResetGame();
        }
    }

    // Reset the game for a new command
    void ResetGame()
    {
        playerSequence.Clear();
        GenerateCommand();
        DisplayCommand();
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
    }

}
