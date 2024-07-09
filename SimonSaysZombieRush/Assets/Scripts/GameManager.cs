using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private GameObject menuActive;
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject menuWin;
    [SerializeField] private GameObject menuLose;

    public GameObject player;
    public Player playerScript;
    public bool isPaused;

    [SerializeField] private int commandLength = 3; // Length of the command sequence
    [SerializeField] private TextMeshProUGUI commandDisplay; // TextMeshProUGUI to display the command
    [SerializeField] private TextMeshProUGUI resultDisplay; // TextMeshProUGUI to display the result

    private List<string> possibleItems; // List of possible items
    private List<string> commandSequence; // The generated command sequence
    private List<string> playerSequence; // The player's collected sequence

    private float initialTimeScale;
    private int enemyCount = 0;

    void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<Player>();
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

    public void UpdateGameGoal(int amount)
    {
        enemyCount += amount;

        if (enemyCount <= 0)
        {
            WinGame();
        }
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
        

        for (int i = 0; i < playerSequence.Count; i++)
        {
            if (playerSequence[i] != commandSequence[i])
            {
                StartCoroutine(ShowResult("Result: Incorrect sequence!"));
                ResetGame();
                return;
            }
        }

        if (playerSequence.Count == commandSequence.Count)
        {
            StartCoroutine(ShowResult("Result: Correct sequence!"));
            ResetGame();
        }

        if (possibleItems.Count == 0)
        {
            StartCoroutine(ShowResultAndWin("Result: All items collected"));
        }
    }

    // Reset the game for a new command
    void ResetGame()
    {
        playerSequence.Clear();
        InitializePossibleItems();
        GenerateCommand();
        DisplayCommand();
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