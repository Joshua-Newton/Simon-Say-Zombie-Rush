using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class HumanScavenger : EnemyShooter
{
    [Header("---- Territory -----")]
    [SerializeField] protected float territoryDistance = 15f;
    [Range(0f, 180f)] [SerializeField] protected float swivelAngle = 90f;
    [Range(0f, 60f)] [SerializeField] protected float seekAttackerTime = 5f;

    [Header("----- Scavenger Audio -----")]
    [SerializeField] AudioClip[] detectSounds;
    [Range(0, 10)][SerializeField] float detectVolume = 0.5f;

    GameObject supplyItem;
    bool foundItem;
    bool isSwiveling;
    bool isSeekingAttacker;
    bool swivelDirection;
    float playerDistFromSupply;
    float randomOffset;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        UpdatePlayerPositionData();
        if(foundItem)
        {
            ProtectSupply();
        }
        Search();
        AggroEnemy();
        Swivel();
        CheckIfItemStillExists();
    }

    protected void Search()
    {
        if (!playerInRange && !foundItem && !isRoaming && agent.remainingDistance < 0.05f)
        {
            StartCoroutine(Roam());
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        // If the trigger is a pickup and the scavenger can see the item, then move to that item
        Vector3 otherDirection = headPos.position - other.transform.position;

        RaycastHit hit;
        if (other.CompareTag("Pickup") && Physics.Raycast(headPos.position, otherDirection, out hit))
        {
            StopCoroutine(Roam());
            isRoaming = false;
            foundItem = true;
            supplyItem = other.gameObject;
            agent.stoppingDistance = 0;
            agent.SetDestination(other.gameObject.transform.position);
        }

        if (other.CompareTag("Enemy") && Physics.Raycast(headPos.position, otherDirection, out hit))
        {
            // TODO: Implement Scavenger attacking zombies
            //Debug.Log("Enemy detected");
        }
        
        if (other.CompareTag("Player") && CanSeePlayerWithoutMovingOrAttacking())
        {
            StartCoroutine(PlayDetectionAudio());
        }

    }

    protected void AggroEnemy()
    {
        // TODO: Expand to attack any target (i.e. zombies or the player)
        if(playerInRange && CanSeePlayerWithoutMovingOrAttacking())
        {
            Move();
            Attack();
            agent.stoppingDistance = stoppingDistOrig;
        }

        
    }

    protected void ProtectSupply()
    {
        playerDistFromSupply = (GameManager.instance.player.transform.position - supplyItem.transform.position).magnitude;
        playerInRange = (playerDistFromSupply < territoryDistance);

        if(!playerInRange || !CanSeePlayerWithoutMovingOrAttacking() || !isSeekingAttacker)
        {
            agent.stoppingDistance = 2;
            agent.SetDestination(supplyItem.transform.position);
            
        }
    }

    protected void Swivel()
    {

        if (agent.remainingDistance <= 0.05f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(transform.rotation.x, transform.rotation.y + randomOffset, transform.rotation.z), faceTargetSpeed * Time.deltaTime);
            if (!isSwiveling)
            {
                StartCoroutine(SwivelDirection());
            }
        }
    }

    protected void CheckIfItemStillExists()
    {
        if(supplyItem == null || !supplyItem.activeSelf)
        {
            foundItem = false;
        }
    }

    IEnumerator SwivelDirection()
    {
        isSwiveling = true;
        if (swivelDirection)
        {
            randomOffset = Random.Range(0, swivelAngle);
        }
        else
        {
            randomOffset = Random.Range(-swivelAngle, 0);
        }
        swivelDirection = !swivelDirection;
        yield return new WaitForSeconds(2f);
        isSwiveling = false;
    }

    protected override void Attack()
    {
        base.Attack();
    }

    public override void TakeDamage(int amount, string damageSource = "")
    {
        HP -= amount;
        StartCoroutine(FlashDamage());
        if (HP <= 0)
        {
            Die();
        }
        if (agent.isOnNavMesh && !isSeekingAttacker)
        {
            StartCoroutine(SeekAttacker());
        }
    }

    IEnumerator SeekAttacker()
    {
        isSeekingAttacker = true;
        agent.SetDestination(GameManager.instance.player.transform.position);
        yield return new WaitForSeconds(seekAttackerTime);
        isSeekingAttacker = false;
    }

    IEnumerator PlayDetectionAudio()
    {
        audSource.PlayOneShot(detectSounds[Random.Range(0, detectSounds.Length)], detectVolume);
        yield return new WaitForSeconds(0.5f);
    }

}
