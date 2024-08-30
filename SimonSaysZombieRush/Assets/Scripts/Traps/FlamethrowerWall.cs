using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerWall : MonoBehaviour
{
    [SerializeField] int flameRest;
    [SerializeField] int flameTime;
    [SerializeField] float sparkTime;
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
            StartCoroutine(sparksTimer());
            StartCoroutine(flamehitBoxTimer());
        }
    }

    IEnumerator flamehitBoxTimer()
    {
        flamethrower.Play(true);
        isFlaming = true;
        flameHitBox.gameObject.SetActive(true);
        yield return new WaitForSeconds(flameTime);
        flamethrower.Stop();
        flameHitBox.gameObject.GetComponent<Damage>().DisableAndStopCoroutines();
        yield return new WaitForSeconds(flameRest);
        isFlaming = false;
    }

    IEnumerator sparksTimer()
    {
        flameSparks.Play(true);
        yield return new WaitForSeconds((int)sparkTime);
        flameSparks.Stop();
    }
}
