using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] possibleSpawnObjects;
    [SerializeField] Transform[] possibleSpawnTransforms;
    [SerializeField] int spawnCooldown; // Time in between each spawn
    [SerializeField] int numToSpawnSimultaneously; // How many to spawn at once
    [SerializeField] public int maxAlive; // Max number of spawns on the map at once
    [SerializeField] public int maxSpawns; // The Max number that this spawner will spawn before being reset

    int numAlive; // The number currently alive
    int numSpawned; // The number this spawner has spawned
    int numKilled; // The number of enemies killed from this spawner
    bool isSpawning; // Whether this spawner will start spawning objects

    // Spawns a single random object at a random transform from the spawner's arrays
    private void SpawnSingle()
    {
        if(CanSpawn())
        {
            IncrementEnemyNumbers();
            GameObject objectToSpawn = possibleSpawnObjects[Random.Range(0, possibleSpawnObjects.Length)];
            Transform transformToSpawnAt = possibleSpawnTransforms[Random.Range(0, possibleSpawnTransforms.Length)];
            SetEnemySpawner(Instantiate(objectToSpawn, transformToSpawnAt.position, transformToSpawnAt.rotation));
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
        return numAlive < maxAlive && numSpawned < maxSpawns;
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
        if (isSpawning)
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

    private void IncrementEnemyNumbers()
    {
        ++numAlive;
        ++numSpawned;
    }

    public void ResetSpawner()
    {
        numAlive = 0;
        numSpawned = 0;
        numKilled = 0;
        isSpawning = false;
    }

    public void EnemyKilled()
    {
        ++numKilled;
        --numAlive;
        if(numKilled >= maxSpawns && HordeModeManager.instance)
        {
            HordeModeManager.instance.StartNextWave();
        }
    }
    

    IEnumerator Spawn()
    {
        for(int i = 0; i < numToSpawnSimultaneously; i++)
        {
            SpawnSingle();
        }
        if(numSpawned >= maxSpawns)
        {
            StopSpawning();
        }
        yield return new WaitForSeconds(spawnCooldown);
    }

}
