using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HordeModeManager : GameManager
{

    public static new HordeModeManager instance;

    [Header("***** START HordeModeManager *****")]

    [Header("----- Wave Parameters -----")]
    [SerializeField] bool infiniteWaves;
    [SerializeField] int totalWaves;
    [SerializeField] int secondsBetweenWaves;
    [Range(1, 100)] [SerializeField] int maxEnemiesInWave = 18;
    [Range(1, 20)] [SerializeField] int enemyMultiplier = 5;

    [Header("----- Wave UI -----")]
    [SerializeField] TMP_Text waveCountText;

    [Header("----- Found Dependencies -----")]
    [Header("EVERY HORDE MANAGER NEEDS A RANDOM SPAWNER TO EXIST IN THE SCENE, IT WILL BE FOUND ON RUNTIME")]
    [SerializeField] RandomSpawner enemySpawner;

    int currentWave;
    int enemiesInWave;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        enemySpawner = FindObjectOfType<RandomSpawner>();
    }

    public void Start()
    {
        
        StartCoroutine(StartWave());
    }

    public override void CollectItem(GameObject item)
    {
        Debug.Log("Picked up: " + item.name);
    }

    public override void LoseGame()
    {
        menuActive = menuLose;
        PauseAndOpenActiveMenu();
        SaveStats();
    }

    public override void WinGame()
    {
        menuActive = menuWin;
        PauseAndOpenActiveMenu();
        SaveStats();
    }

    public void StartNextWave()
    {
        StartCoroutine(StartWave());
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public void UpdateWaveCount()
    {
        waveCountText.text = currentWave.ToString("F0");
    }

    IEnumerator StartWave()
    {
        ++currentWave;
        UpdateWaveCount();
        enemiesInWave = currentWave * enemyMultiplier; // TODO: Replace with a more complex system. For now just have the player kill as many enemies as are in the wave
        yield return new WaitForSeconds(secondsBetweenWaves);

        if (!infiniteWaves && currentWave <= totalWaves)
        {
            ResetAndStartSpawning();
        }
        else if(!infiniteWaves && currentWave > totalWaves)
        {
            WinGame();
        }
        else // Only case left is if infiniteWaves is true
        {
            ResetAndStartSpawning();
        }
    }

    private void ResetAndStartSpawning()
    {
        enemySpawner.ResetSpawner();
        enemySpawner.maxSpawns = Mathf.Min(enemiesInWave, maxEnemiesInWave);
        enemySpawner.StartSpawning();
    }

    void SaveStats()
    {
        HordeStats currentStats = (HordeStats)levelStats;
        if(currentStats != null && currentWave > currentStats.HighestWave)
        {
            UpdateStats(currentStats);
        }
    }

    public override void UpdateStats(LevelStats stats)
    {
        if (stats.GetType() == typeof(HordeStats))
        {
            ((HordeStats)stats).HighestWave = currentWave;
        }
        stats.EnemiesKilled = 0; // TODO: Implement enemies killed tracker and assign here
        stats.TimeUsed = 0; // TODO: Implement time tracker
    }

}
