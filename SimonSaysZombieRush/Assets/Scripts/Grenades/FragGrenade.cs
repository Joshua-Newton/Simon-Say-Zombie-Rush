using System.Collections;
using UnityEngine;

public class FragGrenade : GravityGrenade
{
    [SerializeField] private float explosionForce = 700f; // Force of the explosion
    [SerializeField] private int shrapnelCount = 50; // Number of shrapnel rays
    [SerializeField] private int damage = 50; // Damage dealt by each shrapnel
    [SerializeField] GameObject explosionPrefab;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = explosionRadius;
        sphereCollider.enabled = false;

        StartCoroutine(ExplodeAfterDelay());
    }

    IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    void Explode()
    {
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<Explosion>().SetExplosionDamage(damage);

        // Destroy the grenade after the explosion
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Draw a sphere in the editor to visualize the explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}