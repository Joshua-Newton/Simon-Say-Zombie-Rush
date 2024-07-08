using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] enum damageType { bullet, stationary };
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;
    [SerializeField] int damageAmount;
    [SerializeField] int speed;
    [SerializeField] int destroyTime;

    bool hasDamaged;

    // Start is called before the first frame update
    void Start()
    {
        if(type == damageType.bullet)
        {
            rb.velocity = transform.forward * speed;
            Destroy(gameObject, destroyTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.isTrigger)
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
    }
}
