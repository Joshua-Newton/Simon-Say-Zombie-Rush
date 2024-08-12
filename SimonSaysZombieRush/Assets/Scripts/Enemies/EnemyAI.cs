using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyAI : MonoBehaviour, IDamage
{
    [Header("----- Model -----")]
    [SerializeField] protected Renderer model;
    [SerializeField] protected Color hitColor;

    [Header("----- Stats -----")]
    [SerializeField] protected int HP;

    [Header("----- Navigation -----")]
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected int faceTargetSpeed;
    [SerializeField] protected bool alwaysChasePlayer;
    [SerializeField] protected int roamDistance;
    [SerializeField] protected int roamTimer;

    [Header("----- Player Detection -----")]
    [SerializeField] protected Transform headPos;
    [SerializeField] protected int viewAngle;

    [Header("----- Animation -----")]
    [SerializeField] protected Animator anim;
    [SerializeField] protected int animSpeedTransition;

    [Header("----- Grenade Effect -----")]
    public bool isGrenadeEffectActive = false;

    [Header("----- Spawn Source -----")]
    // Replace with a system that take advantage of inheritance so that we only need one of these
    public WaveSpawner sourceWaveSpawner;
    public RandomSpawner sourceRandomSpawner;

    [Header("----- Audio -----")]
    [SerializeField] AudioSource audSource;
    [SerializeField] AudioClip[] groanSounds;
    [Range(8, 100)] [SerializeField] float minTimeBetweenSounds = 5;
    [Range(0, 5)] [SerializeField] float timeVariance = 1;
    [Range(0, 1)] [SerializeField] float groanVolume = 0.3f;

    [Header("----- Drops -----")]
    [SerializeField] bool canDropItems;
    [SerializeField] GameObject[] possibleDrops;
    [Range(0, 100)] [SerializeField] float dropChance;

    protected Color originalColor;
    protected bool isRoaming;
    protected bool playerInRange;
    protected float angleToPlayer;
    protected float stoppingDistOrig;
    protected Vector3 playerDir;
    protected Vector3 startingPos;
    protected bool isSpeaking;
    private bool isStunned = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;
        GameManager.instance.UpdateEnemyCount(1);
        originalColor = model.material.color;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        float agentSpeed = agent.velocity.normalized.magnitude;
        anim.SetFloat("Speed", Mathf.Lerp(anim.GetFloat("Speed"), agentSpeed, Time.deltaTime * animSpeedTransition));

        playerDir = GameManager.instance.player.transform.position - headPos.position;
        angleToPlayer = Vector3.Angle(playerDir, transform.forward);

        if (alwaysChasePlayer)
        {
            Move();
            if (playerInRange && angleToPlayer <= viewAngle)
            {
                Attack();
            }
        }
        else if (!playerInRange || (playerInRange && canSeePlayer()))
        {
            if(!isRoaming && agent.remainingDistance < 0.05f)
            {
                StartCoroutine(Roam());
            }
        }

        if(!isSpeaking)
        {
            StartCoroutine(Groan());
        }

    }

    IEnumerator Groan()
    {
        isSpeaking = true;
        audSource.PlayOneShot(groanSounds[Random.Range(0, groanSounds.Count())], groanVolume);
        yield return new WaitForSeconds(minTimeBetweenSounds + Random.Range(-timeVariance, timeVariance));
        isSpeaking = false;
    }

    IEnumerator Roam()
    {
        isRoaming = true;
        yield return new WaitForSeconds(roamTimer);

        agent.stoppingDistance = 0;
        Vector3 ranPos = Random.insideUnitSphere * roamDistance;
        ranPos += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDistance, 1);
        agent.SetDestination(hit.position);

        isRoaming = false;
    }

    bool canSeePlayer()
    {
        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit) || alwaysChasePlayer)
        {
            if ((hit.collider.CompareTag("Player") && angleToPlayer <= viewAngle) || alwaysChasePlayer)
            {
                Move();
                Attack();
                agent.stoppingDistance = stoppingDistOrig;
                return true;
            }

        }
        agent.stoppingDistance = 0;
        return false;
    }

    private void Move()
    {
        if (!isGrenadeEffectActive)
        {
            agent.SetDestination(GameManager.instance.player.transform.position);
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            FaceTarget();
        }
    }

    protected abstract void Attack();
    
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
            agent.stoppingDistance = 0;
            playerInRange = false;
        }
    }

    public void TakeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(FlashDamage());
        if (HP <= 0)
        {
            Die();
        }
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(GameManager.instance.player.transform.position);
        }
    }

    public void Stun(float duration)
    {
        if (!isStunned)
        {
            StartCoroutine(StunCoroutine(duration));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        agent.isStopped = true; // Stop the NavMeshAgent to disable movement
        anim.enabled = false; // Optionally disable animations

        yield return new WaitForSeconds(duration);

        agent.isStopped = false; // Re-enable the NavMeshAgent
        anim.enabled = true; // Optionally re-enable animations
        isStunned = false;
    }

    private void Die()
    {
        GameManager.instance.UpdateEnemyCount(-1);
        if(TimeTrialModeManager.instance != null)
        {
            TimeTrialModeManager.instance.UpdateScoreFromKill();
        }

        if(sourceWaveSpawner)
        {
            sourceWaveSpawner.UpdateEnemyNumber();
        }
        if(sourceRandomSpawner)
        {
            sourceRandomSpawner.EnemyKilled();
        }

        if(canDropItems && Random.Range(0, 99) < dropChance)
        {
            DropRandomItem();
        }


        Destroy(gameObject);
    }

    void DropRandomItem()
    {
        GameObject itemDrop = possibleDrops[Random.Range(0, possibleDrops.Length)];
        if (itemDrop != null)
        {
            Instantiate(itemDrop, transform.position + Vector3.up, Quaternion.identity);
        }
    }

    void FaceTarget()
    {
        Quaternion rotation = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, faceTargetSpeed * Time.deltaTime);
    }

    IEnumerator FlashDamage()
    {
        model.material.color = hitColor;
        yield return new WaitForSeconds(.1f);
        model.material.color = originalColor;
    }
}
