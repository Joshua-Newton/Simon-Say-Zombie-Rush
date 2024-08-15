using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class HumanScavenger : EnemyShooter
{
    [Header("---- Territory -----")]
    [SerializeField] protected float territoryDistance = 15f;
    [Range(0,180)] [SerializeField] protected float swivelAngle = 90f;

    GameObject supplyItem;
    bool foundItem;
    bool isSwiveling;
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
            Debug.Log("Enemy detected");
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

        if(!playerInRange || !CanSeePlayerWithoutMovingOrAttacking())
        {
            agent.stoppingDistance = 0;
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


}
