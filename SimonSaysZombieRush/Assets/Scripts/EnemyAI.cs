using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamage
{
    [SerializeField] Renderer model;
    [SerializeField] Color hitColor;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject spitProjectile;
    [SerializeField] Transform shootPos;
    [SerializeField] float shootDelay;
    [SerializeField] float spitDelay;
    [SerializeField] float meleeHitTime;
    [SerializeField] float meleeDelay;
    [SerializeField] float meleeTriggerRange;

    [SerializeField] int HP;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int viewAngle;
    [SerializeField] Collider meleeHitBox;

    [SerializeField] enum EnemyType { Shooter, Melee, Spitter };
    [SerializeField] EnemyType enemyType;

    Color originalColor;
    bool isShooting;
    bool isMeleeing;
    bool isSpitting;
    bool playerInRange;
    float angleToPlayer;
    Vector3 playerDir;
    public bool isGrenadeEffectActive = false;

    public WaveSpawner sourceWaveSpawner;

    // Start is called before the first frame update
    void Start()
    {
        if(meleeHitBox != null)
        {
            meleeHitBox.enabled = false;
        }
        GameManager.instance.UpdateEnemyCount(1);
        originalColor = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInRange && canSeePlayer())
        {
            

        }
    }

    bool canSeePlayer()
    {
        playerDir = GameManager.instance.player.transform.position - transform.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        Debug.DrawRay(transform.position, playerDir);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewAngle)
            {
                // Verifica si el efecto de la granada está activo antes de establecer un nuevo destino
                if (!isGrenadeEffectActive)
                {
                    agent.SetDestination(GameManager.instance.player.transform.position);
                }

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    FaceTarget();
                }

                if (enemyType == EnemyType.Shooter && !isShooting)
                {
                    StartCoroutine(Shoot());
                }
                else if (enemyType == EnemyType.Melee && !isMeleeing && playerDir.magnitude < meleeTriggerRange)
                {
                    StartCoroutine(Melee());
                }
                else if (enemyType == EnemyType.Spitter && !isSpitting)
                {
                    StartCoroutine(Spit());
                }

                return true;
            }

        }

        return false;
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    public void TakeDamage(int amount)
    {
        HP -= amount;
        agent.SetDestination(GameManager.instance.player.transform.position);
        StartCoroutine(FlashDamage());
        if (HP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameManager.instance.UpdateEnemyCount(-1);

        if(sourceWaveSpawner)
        {
            sourceWaveSpawner.UpdateEnemyNumber();
        }

        Destroy(gameObject);
    }

    void FaceTarget()
    {
        Quaternion rotation = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, faceTargetSpeed * Time.deltaTime);
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        Instantiate(bullet, shootPos.position, transform.rotation);
        yield return new WaitForSeconds(shootDelay);
        isShooting = false;
    }

    IEnumerator Melee()
    {
        isMeleeing = true;
        meleeHitBox.enabled = true;
        yield return new WaitForSeconds(meleeHitTime);
        meleeHitBox.enabled = false;
        yield return new WaitForSeconds(meleeDelay);
        isMeleeing = false;
    }

    IEnumerator FlashDamage()
    {
        model.material.color = hitColor;
        yield return new WaitForSeconds(.1f);
        model.material.color = originalColor;
    }

    IEnumerator Spit()
    {
        isSpitting = true;
        Instantiate(spitProjectile, shootPos.position, transform.rotation);
        yield return new WaitForSeconds(spitDelay);
        isSpitting = false;
    }

}
