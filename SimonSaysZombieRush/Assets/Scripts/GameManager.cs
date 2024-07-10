using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject menuActive;
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject menuWin;
    [SerializeField] private GameObject menuLose;
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] TMP_Text scoreText; // TextMeshProUGUI to display the score

    public Image playerHPBar;
    public GameObject dmgFlashBckgrnd;
    public GameObject player;
    public Player playerScript;
    public bool isPaused;
    public Collider playerCollider;

    [SerializeField] private int commandLength = 3; // Length of the command sequence
    [SerializeField] private TextMeshProUGUI commandDisplay; // TextMeshProUGUI to display the command
    [SerializeField] private TextMeshProUGUI resultDisplay; // TextMeshProUGUI to display the result

    private List<string> possibleItems; // List of possible items
    private List<string> commandSequence; // The generated command sequence
    private List<string> playerSequence; // The player's collected sequence

    private float initialTimeScale;
    private int enemyCount;
    private int score; // Player's score

    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<Player>();
        playerCollider = player.GetComponent<Collider>();
        initialTimeScale = Time.timeScale;
    }

    void Start()
    {
        InitializePossibleItems();
        GenerateCommand();
        DisplayCommand();
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                menuActive = menuPause;
                PauseAndOpenActiveMenu();
            }
            else if (menuActive == menuPause)
            {
                StateUnpause();
            }
        }
    }

    public void StatePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void StateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = initialTimeScale;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(isPaused);
        menuActive = null;
    }

    public void UpdateEnemyCount(int amount)
    {
        enemyCount += amount;
        enemyCountText.text = enemyCount.ToString("F0");
    }

    public void LoseGame()
    {
        menuActive = menuLose;
        PauseAndOpenActiveMenu();
    }

    public void WinGame()
    {
        menuActive = menuWin;
        PauseAndOpenActiveMenu();
    }

    void PauseAndOpenActiveMenu()
    {
        StatePause();
        menuActive.SetActive(true);
    }

    // Initialize the possible items list based on objects with the tag "Pickup" in the scene
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
    public void CollectItem(string item)
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
    }

    // Coroutine to show the result for 2 seconds
    IEnumerator ShowResult(string message)
    {
        resultDisplay.text = message;
        resultDisplay.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        resultDisplay.gameObject.SetActive(false);
    }

    // Coroutine to show the result and then display the win menu
    IEnumerator ShowResultAndWin(string message)
    {
        resultDisplay.text = message;
        resultDisplay.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        resultDisplay.gameObject.SetActive(false);
        WinGame();
    }

}
