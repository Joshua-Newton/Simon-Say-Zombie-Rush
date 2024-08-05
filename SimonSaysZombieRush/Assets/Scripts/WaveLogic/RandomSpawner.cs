using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] possibleSpawnObjects;
    [SerializeField] Transform[] possibleSpawnTransforms;
    [SerializeField] int spawnCooldown; // Time in between each spawn
    [SerializeField] int numToSpawnSimultaneously; // How many to spawn at once
    [SerializeField] public int maxAlive; // Max number of spawns on the map at once
    [SerializeField] public int maxSpawns; // The Max number that this spawner will spawn before being reset
    [SerializeField] bool increaseEnemySpeedWithWaves;
    [Range(0, 1)] [SerializeField] float speedIncreasePerWave = 0.5f;
    [Range(1, 10)][SerializeField] float maxSpeedMultiplier = 5; // how much faster can each enemy go

    int numSpawned; // The number this spawner has spawned
    int numKilled; // The number of enemies killed from this spawner
    bool isSpawning; // Whether this spawner will start spawning objects
    bool isOnCooldown; // Whether the spawner is cooling down from the spawning the last batch of enemies

    // Spawns a single random object at a random transform from the spawner's arrays
    private void SpawnSingle()
    {
        if(CanSpawn())
        {
            GameObject objectToSpawn = possibleSpawnObjects[Random.Range(0, possibleSpawnObjects.Length)];
            Transform transformToSpawnAt = possibleSpawnTransforms[Random.Range(0, possibleSpawnTransforms.Length)];
            GameObject spawnedObject = Instantiate(objectToSpawn, transformToSpawnAt.position, transformToSpawnAt.rotation);
            IncrementEnemyNumbers();
            SetEnemySpawner(spawnedObject);
            if (increaseEnemySpeedWithWaves)
            {
                IncreaseEnemySpeed(spawnedObject);
            }
        }
    }

    // Spawns a single given object at a random transform from this spawner
    public void SpawnSingle(GameObject objectToSpawn)
    {
        if(CanSpawn())
        {
            IncrementEnemyNumbers();
            Transform transformToSpawnAt = possibleSpawnTransforms[Random.Range(0, possibleSpawnTransforms.Length)];
            SetEnemySpawner(Instantiate(objectToSpawn, transformToSpawnAt.position, transformToSpawnAt.rotation));
        }
    }

    // Spawns a single given object at a given transform
    public void SpawnSingle(GameObject objectToSpawn, Transform transformToSpawnAt)
    {
        if (CanSpawn())
        {
            IncrementEnemyNumbers();
            SetEnemySpawner(Instantiate(objectToSpawn, transformToSpawnAt.position, transformToSpawnAt.rotation));
        }
    }

    private bool CanSpawn()
    {
        return HordeModeManager.instance.GetEnemyCount() < maxAlive && numSpawned < maxSpawns;
    }

    public void StartSpawning()
    {
        isSpawning = true;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    private void Update()
    {
        if (isSpawning && !isOnCooldown)
        {
            StartCoroutine(Spawn());
        }
    }

    private void SetEnemySpawner(GameObject targetObject)
    {
        EnemyAI enemyComponent = targetObject.GetComponent<EnemyAI>();
        if (enemyComponent != null)
        {
            enemyComponent.sourceRandomSpawner = this;
        }
    }

    private void IncreaseEnemySpeed(GameObject targetObject)
    {
        NavMeshAgent enemyAgent = targetObject.GetComponent<NavMeshAgent>();
        if(enemyAgent != null)
        {
            float maxMultiplier = Mathf.Min(maxSpeedMultiplier, 1 + ((float)HordeModeManager.instance.GetCurrentWave() * speedIncreasePerWave));
            enemyAgent.speed *= Random.Range(1, maxMultiplier);
        }
    }

    private void IncrementEnemyNumbers()
    {
        ++numSpawned;
    }

    public void ResetSpawner()
    {
        numSpawned = 0;
        numKilled = 0;
        isSpawning = false;
        //isOnCooldown = false;
    }

    public void EnemyKilled()
    {
        ++numKilled;
        if(numKilled >= maxSpawns && HordeModeManager.instance)
        {
            HordeModeManager.instance.StartNextWave();
        }
    }
    

    IEnumerator Spawn()
    {
        isOnCooldown = true;
        for(int i = 0; i < numToSpawnSimultaneously; i++)
        {
            SpawnSingle();
        }
        if(numSpawned >= maxSpawns)
        {
            StopSpawning();
        }
        yield return new WaitForSeconds(spawnCooldown);
        isOnCooldown = false;
    }

}
