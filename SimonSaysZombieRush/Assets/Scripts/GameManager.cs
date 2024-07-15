using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor;

public abstract class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // TODO: Combine menuWin and menuWinLastLevel and just programmatically change the buttons
    [SerializeField] protected GameObject menuActive;
    [SerializeField] protected GameObject menuPause;
    [SerializeField] protected GameObject menuWin;
    [SerializeField] protected GameObject menuWinLastLevel;
    [SerializeField] protected GameObject menuLose;
    [SerializeField] protected TMP_Text enemyCountText;
    [SerializeField] protected TMP_Text scoreText; // TextMeshProUGUI to display the score
    [SerializeField] protected TMP_Text scoreTextFinalLevel; // TextMeshProUGUI to display the score

    public Image playerHPBar;
    public GameObject dmgFlashBckgrnd;
    public GameObject player;
    public Player playerScript;
    public bool isPaused;
    public Collider playerCollider;

    protected float initialTimeScale;
    protected int enemyCount;
    protected int score; // Player's score

    // Name of the next scene to load
    protected string nextScenePath;
    protected bool lastLevel;

    protected virtual void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<Player>();
        playerCollider = player.GetComponent<Collider>();

        initialTimeScale = Time.timeScale;
        int currentBuildIndex = SceneUtility.GetBuildIndexByScenePath(SceneManager.GetActiveScene().path);
        if (currentBuildIndex < SceneManager.sceneCountInBuildSettings - 1)
        {
            nextScenePath = SceneUtility.GetScenePathByBuildIndex(currentBuildIndex + 1);
        }
        else
        {
            lastLevel = true;
        }
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

    public abstract void LoseGame();

    public abstract void WinGame();

    public abstract void CollectItem(string item);

    protected void PauseAndOpenActiveMenu()
    {
        StatePause();
        menuActive.SetActive(true);
    }

    // Initialize the possible items list based on objects with the tag "Pickup" in the scene
   
}
