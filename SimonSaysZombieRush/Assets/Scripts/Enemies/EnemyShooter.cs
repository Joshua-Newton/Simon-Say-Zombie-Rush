using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : EnemyAI
{
    [Header("----- Shooting -----")]
    [SerializeField] protected GameObject bullet;
    [SerializeField] protected Transform shootPos;
    [SerializeField] protected float shootDelay;
    protected bool isShooting;

    [SerializeField] AudioClip shootAudio;
    [Range(0, 1)][SerializeField] float shootAudioVolume = 0.5f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void Attack()
    {
        if(!isShooting)
        {
            StartCoroutine(Shoot());
        }
    }
    IEnumerator Shoot()
    {
        isShooting = true;
        Instantiate(bullet, shootPos.position, transform.rotation);
        audSource.PlayOneShot(shootAudio, shootAudioVolume);
        yield return new WaitForSeconds(shootDelay);
        isShooting = false;
    }

}
