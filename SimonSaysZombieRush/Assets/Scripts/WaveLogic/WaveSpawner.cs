using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] int numToSpawn;
    [SerializeField] int spawnTimer;
    [SerializeField] Transform[] spawnPos;

    int spawnCount;
    bool isSpawning;
    bool startSpawning;
    int numKilled;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(startSpawning && !isSpawning && spawnCount < numToSpawn)
        {
            StartCoroutine(Spawn());
        }
    }

    public void StartWave()
    {
        startSpawning = true;
        GameManager.instance.UpdateEnemyCount(numToSpawn);
    }

    IEnumerator Spawn()
    {
        isSpawning = true;
        int arrayPos = Random.Range(0, spawnPos.Length);
        GameObject objectSpawned = Instantiate(objectToSpawn, spawnPos[arrayPos].position, spawnPos[arrayPos].rotation);

        EnemyAI component = objectSpawned.GetComponent<EnemyAI>();
        if (component != null )
        {
            component.sourceWaveSpawner = this;
        }

        ++spawnCount;
        yield return new WaitForSeconds(spawnTimer);
        isSpawning = false;
    }

    public void UpdateEnemyNumber()
    {
        ++numKilled;

        if (numKilled >= numToSpawn)
        {
            startSpawning = false;
            StartCoroutine(WaveManager.instance.StartWave());
        }
    }

}
