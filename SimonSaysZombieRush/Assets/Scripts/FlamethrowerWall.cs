using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerWall : MonoBehaviour
{
    [SerializeField] int flameRest;
    [SerializeField] int flameTime;
    [SerializeField] Collider flameHitBox;
    [SerializeField] ParticleSystem flameSparks;
    [SerializeField] ParticleSystem flamethrower;
    bool isFlaming;


    // Update is called once per frame
    void Update()
    {
        flamethowerEmission();
    }

    public void flamethowerEmission()
    {
        if (!isFlaming)
        {
            StartCoroutine(flamehitBoxTimer());
        }
    }

    IEnumerator flamehitBoxTimer()
    {
        flamethrower.Play(true);
        flameSparks.Play(true);
        isFlaming = true;
        flameHitBox.gameObject.SetActive(true);
        yield return new WaitForSeconds(flameTime);
        flameSparks.Stop();
        flamethrower.Stop();
        flameHitBox.gameObject.SetActive(false);
        yield return new WaitForSeconds(flameRest);
        isFlaming = false;
    }
}
