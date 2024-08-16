using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] protected enum damageType { bullet, stationary, spit, flame, lava };
    [SerializeField] protected damageType type;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected int damageAmount;
    [SerializeField] protected int speed;
    [SerializeField] protected int destroyTime;
    [SerializeField] protected GameObject acid;
    [SerializeField] protected float acidDuration;
    [SerializeField] protected bool repeatDamage;
    [SerializeField] protected float repeatDelay;
    [SerializeField] protected bool damagePlayer = true;


    protected bool hasDamaged;
    protected bool isDamaging;
    protected bool isBurning;
    protected Coroutine spitCoroutine;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        if(type == damageType.bullet)
        {
            rb.velocity = transform.forward * speed;
            Destroy(gameObject, destroyTime);
        }
        else if(type == damageType.spit)
        {
            rb.velocity = transform.forward * speed;
            spitCoroutine = StartCoroutine(spit());
        }

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        
        if(other.isTrigger || 
            repeatDamage ||
            (!damagePlayer && other.CompareTag("Player")))
        {
            return;
        }

        IDamage damageTarget = other.GetComponent<IDamage>();

        if(damageTarget != null && !hasDamaged)
        {
            damageTarget.TakeDamage(damageAmount);
        }

        if(type == damageType.bullet)
        {
            hasDamaged = true;
            Destroy(gameObject);
        }
        else if(type == damageType.spit)
        {
            StopCoroutine(spitCoroutine);
            SpawnAcid();
        }
    }

    void OnTriggerStay (Collider other)
    {
        if (other.isTrigger || other.CompareTag("Enemy"))
        {
            return; // ignore enemies and other triggers for ontriggerstay interactions
        }

        if ((type == damageType.stationary || type == damageType.flame || type == damageType.lava) && repeatDamage && !isDamaging)
        {
            IDamage damageTarget = other.GetComponent<IDamage>();
            if(damageTarget != null)
            {
                StartCoroutine(repeatingDamage(damageTarget));
            }
        }

    }

    IEnumerator spit()
    {
        yield return new WaitForSeconds(destroyTime);
        SpawnAcid();
    }

    void SpawnAcid()
    {
        GameObject acidInstance;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            acidInstance = Instantiate(acid, hit.point, Quaternion.identity);
        }
        else
        {
            acidInstance = Instantiate(acid, transform.position, Quaternion.identity);
        }

        Destroy(acidInstance.gameObject, acidDuration);
        Destroy(gameObject);
    }

    IEnumerator repeatingDamage(IDamage damageTarget)
    {
        isDamaging = true;
        damageTarget.TakeDamage(damageAmount);
        yield return new WaitForSeconds(repeatDelay);
        isDamaging = false;
    }

    public void SetDamage(int newDamage)
    {
        damageAmount = newDamage;
    }
}
