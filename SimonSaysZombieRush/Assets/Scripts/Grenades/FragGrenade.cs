using System.Collections;
using UnityEngine;

public class FragGrenade : MonoBehaviour
{
    [SerializeField] private float explosionDelay = 3f; // Delay before the explosion
    [SerializeField] private float explosionRadius = 5f; // Radius of the explosion
    [SerializeField] private float explosionForce = 700f; // Force of the explosion
    [SerializeField] private int shrapnelCount = 50; // Number of shrapnel rays
    [SerializeField] private int damage = 50; // Damage dealt by each shrapnel

    private bool hasExploded = false;
    private Rigidbody rb;
    private SphereCollider sphereCollider;

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
        if (hasExploded) return;

        hasExploded = true;

        // Enable the SphereCollider to detect enemies within the radius
        sphereCollider.enabled = true;

        // Apply explosion force to nearby objects with rigidbodies
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }

        // Simulate shrapnel damage
        for (int i = 0; i < shrapnelCount; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere;
            Ray ray = new Ray(transform.position, randomDirection);
            if (Physics.Raycast(ray, out RaycastHit hit, explosionRadius))
            {
                IDamage damageable = hit.collider.GetComponent<IDamage>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
            }
        }

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