using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunGrenade : MonoBehaviour
{
    [SerializeField] private float explosionDelay = 3f; // Delay before the explosion
    [SerializeField] private float explosionRadius = 5f; // Radius of the explosion
    [SerializeField] private float stunDuration = 5f; // Duration of the stun effect

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

        // Apply stun effect to nearby enemies
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            IDamage damageable = nearbyObject.GetComponent<IDamage>();
            if (damageable != null)
            {
                damageable.Stun(stunDuration);
            }
        }

        // Destroy the grenade after the explosion
        Destroy(gameObject, 0.1f); // Slight delay to ensure all coroutines are triggered
    }

    void OnDrawGizmosSelected()
    {
        // Draw a sphere in the editor to visualize the explosion radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
