using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HordeModeManager : GameManager
{
    public static new HordeModeManager instance;
    [SerializeField] bool infiniteWaves;
    [SerializeField] int totalWaves;
    [SerializeField] int secondsBetweenWaves;
    [SerializeField] RandomSpawner enemySpawner;
    [SerializeField] TMP_Text waveCountText;
    [Range(1, 100)] [SerializeField] int maxEnemiesInWave = 18;
    [Range(1, 20)] [SerializeField] int enemyMultiplier = 5;

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
    }

    public override void WinGame()
    {
        menuActive = menuWin;
        PauseAndOpenActiveMenu();
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
}
