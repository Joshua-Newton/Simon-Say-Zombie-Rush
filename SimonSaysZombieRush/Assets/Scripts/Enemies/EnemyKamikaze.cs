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

    bool isExploding;

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
}
