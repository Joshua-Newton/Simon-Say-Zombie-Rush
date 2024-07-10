using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] enum damageType { bullet, stationary, spit };
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;
    [SerializeField] int damageAmount;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;
    [SerializeField] GameObject acid;
    [SerializeField] float acidDuration;
    [SerializeField] bool repeatDamage;
    [SerializeField] float repeatDelay;


    bool hasDamaged;
    bool isDamaging;
    Coroutine spitCoroutine;
    // Start is called before the first frame update
    void Start()
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
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.isTrigger || repeatDamage)
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
        if (other.CompareTag("Enemy"))
        {
            return; // ignore enemies for ontriggerstay interactions
        }

        if (type == damageType.stationary && repeatDamage && !isDamaging)
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
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit))
        {
            GameObject acidInstance = Instantiate(acid, hit.point, Quaternion.identity);
            Destroy(acidInstance.gameObject, acidDuration);
        }

        Destroy(gameObject);
    }

    IEnumerator repeatingDamage(IDamage damageTarget)
    {
        isDamaging = true;
        damageTarget.TakeDamage(damageAmount);
        yield return new WaitForSeconds(repeatDelay);
        isDamaging = false;
    }
}
