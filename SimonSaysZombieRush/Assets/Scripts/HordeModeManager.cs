using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeModeManager : GameManager
{
    public static new HordeModeManager instance;
    [SerializeField] bool infiniteWaves;
    [SerializeField] int totalWaves;
    [SerializeField] int secondsBetweenWaves;
    [SerializeField] RandomSpawner enemySpawner;

    int currentWave;
    int enemiesInWave;
    int enemiesKilled;
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

    public override void CollectItem(string item)
    {
        Debug.Log("Picked up: " + item);
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

    IEnumerator StartWave()
    {
        ++currentWave;
        enemiesInWave = currentWave; // TODO: Replace with a more complex system. For now just have the player kill as many enemies as are in the wave
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
        enemySpawner.maxSpawns = enemiesInWave;
        enemySpawner.StartSpawning();
    }
}
