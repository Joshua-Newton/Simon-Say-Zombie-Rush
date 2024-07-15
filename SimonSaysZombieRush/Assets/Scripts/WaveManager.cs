using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;
    public WaveSpawner[] spawners;
    [SerializeField] int timeBetweenWaves;

    public int waveCurrent;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        StartCoroutine(StartWave()); // TODO: Starts first wave on game start, replace this with a more elegant system
    }

    public IEnumerator StartWave()
    {
        ++waveCurrent;

        if(waveCurrent <= spawners.Length)
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            spawners[waveCurrent - 1].StartWave();
        }
    }

}
