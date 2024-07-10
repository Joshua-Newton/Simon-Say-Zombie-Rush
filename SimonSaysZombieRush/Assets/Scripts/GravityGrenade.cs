using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGrenade : MonoBehaviour
{
    public float explosionDelay = 3f; // Delay before the explosion
    public float explosionRadius = 5f; // Radius of the explosion
    public float explosionForce = 700f; // Force of the explosion
    public float attractionDuration = 2f; // Duration of the gravitational attraction

    private bool hasExploded = false;

    void Start()
    {
        // Start the countdown for the explosion
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

        // Find all colliders in the radius of the explosion
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Apply an outward explosive force
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

                // Start the gravitational attraction
                StartCoroutine(Attract(rb));
            }
        }

        // Destroy the grenade after the explosion
        Destroy(gameObject, attractionDuration + 1f);
    }

    IEnumerator Attract(Rigidbody rb)
    {
        float elapsedTime = 0f;
        Vector3 originalPosition = rb.position;

        while (elapsedTime < attractionDuration)
        {
            elapsedTime += Time.deltaTime;
            float strength = Mathf.Lerp(1f, 0f, elapsedTime / attractionDuration);
            Vector3 direction = (transform.position - rb.position).normalized;
            rb.MovePosition(rb.position + direction * strength * Time.deltaTime);
            yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a sphere in the editor to visualize the explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
