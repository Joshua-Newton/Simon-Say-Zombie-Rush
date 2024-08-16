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
    [Header("Menus")]
    [SerializeField] protected GameObject menuActive;
    [SerializeField] protected GameObject menuPause;
    [SerializeField] protected GameObject menuWin;
    [SerializeField] protected GameObject menuWinLastLevel;
    [SerializeField] protected GameObject menuLose;
    [Header("Text Fields")]
    [SerializeField] protected TMP_Text loseMessage;
    [SerializeField] protected TMP_Text enemyCountText;
    [SerializeField] protected TMP_Text scoreText; // TextMeshProUGUI to display the score
    [SerializeField] protected TMP_Text scoreTextFinalLevel; // TextMeshProUGUI to display the score
    [Header("High Scores")]
    [SerializeField] protected GameObject HighScoreObject;

    [Header("HUD")]
    public Image playerHPBar;
    public GameObject dmgFlashBckgrnd;
    public TMP_Text ammoCurrent, ammoMax;
    public GameObject checkpointPopup;

    [Header("Player")]
    public GameObject player;
    public Player playerScript;
    public Collider playerCollider;
    public GameObject playerSpawnPos;
    [Header("Pause State")]
    public bool isPaused;

    protected float initialTimeScale;
    protected int enemyCount;
    protected int score; // Player's score

    // Name of the next scene to load
    protected string nextScenePath;
    protected bool lastLevel;


    protected string savePath;
    protected string statsAssetName;
    protected string saveFolder = "Assets/GameplayStats/";


    protected virtual void Awake()
    {
        instance = this;
        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<Player>();
        playerCollider = player.GetComponent<Collider>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");

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

        statsAssetName = SceneManager.GetActiveScene().name + ".asset";
        savePath = saveFolder + statsAssetName;
    }

    protected virtual void Update()
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
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        menuActive.SetActive(isPaused);
        menuActive = null;
    }

    public void UpdateEnemyCount(int amount)
    {
        enemyCount += amount;
        enemyCountText.text = enemyCount.ToString("F0");
    }

    public int GetEnemyCount()
    {
        return enemyCount;
    }

    public abstract void LoseGame(string reason);

    public abstract void WinGame();

    public abstract void CollectItem(GameObject item);

    public abstract void UpdateStats(LevelStats stats);

    protected void PauseAndOpenActiveMenu()
    {
        StatePause();
        menuActive.SetActive(true);
    }
   
}
