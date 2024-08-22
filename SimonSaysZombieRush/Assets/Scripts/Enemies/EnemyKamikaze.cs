using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyKamikaze : EnemyAI
{
    [Header("----- Explosion -----")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float distanceToTriggerExplosion;
    [Range(0.05f, 100)] [SerializeField] float explosionDelay;
    [Range(0.05f, 100)] [SerializeField] float explosionCooldown;

    [Header("----- Kamikaze Sounds -----")]
    [Range(0f, 100f)] [SerializeField] float screamDelay = 1f;
    [SerializeField] AudioClip[] screamAudio;
    [Range(0, 1)] [SerializeField] float screamVolume = 0.5f;
    [SerializeField] AudioClip[] preExplosionAudio;
    [Range(0, 1)] [SerializeField] float preExplosionVolume = 0.5f;

    bool isExploding;
    bool isScreaming;
    protected override void Move()
    {
        if (!isExploding)
        {
            base.Move();
            Attack();
        }
        else
        {
            agent.SetDestination(transform.position);
        }
    }

    protected IEnumerator StartExploding()
    {
        isExploding = true;
        if (preExplosionAudio.Length > 0)
        {
            audSource.PlayOneShot(preExplosionAudio[Random.Range(0, preExplosionAudio.Length)], preExplosionVolume);
        }
        yield return new WaitForSeconds(explosionDelay);
        SpawnExplosion();
        yield return new WaitForSeconds(explosionCooldown);
        isExploding = false;
    }

    protected override void Die()
    {
        StopAllCoroutines();
        SpawnExplosion();
        base.Die();
    }

    protected void SpawnExplosion()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }

    protected override void Attack()
    {
        if (!isExploding && (transform.position - GameManager.instance.player.transform.position).magnitude <= distanceToTriggerExplosion)
        {
            StartCoroutine(StartExploding());
        }
    }

    protected override void PlayerDetected()
    {
        base.PlayerDetected();
        if (!isScreaming)
        {        
            StartCoroutine(Scream());
        }
    }

    IEnumerator Scream()
    {
        isScreaming = true;
        AudioClip selectedClip = screamAudio[Random.Range(0, screamAudio.Length)];
        audSource.PlayOneShot(selectedClip, screamVolume);
        yield return new WaitForSeconds(selectedClip.length + screamDelay);
        isScreaming = false;
    }
}
