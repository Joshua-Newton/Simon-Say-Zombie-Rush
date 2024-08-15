using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Rocket : Damage
{
    [SerializeField] int explosionDamage;
    [SerializeField] GameObject explosion;

    // Start is called before the first frame update
    protected override void Start()
    {
        rb.velocity = Vector3.Lerp(Vector3.zero, transform.forward * speed, 1f);
        explosion.GetComponent<Explosion>().SetExplosionDamage(explosionDamage);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
