using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGrenade : MonoBehaviour
{
    [SerializeField] private float explosionDelay; // Delay before the explosion
    [SerializeField] private float explosionRadius; // Radius of the explosion
    [SerializeField] private float explosionForce; // Force of the explosion
    [SerializeField] private float attractionDuration; // Duration of the gravitational attraction
    [SerializeField] private float attractionStrength; // Strength of the gravitational attraction
    [SerializeField] private float floatHeight; // Height to float before the explosion
    [SerializeField] private float floatDuration; // Duration of the float before the explosion
    [SerializeField] private float elapsedTime;

    private bool hasExploded = false;
    private Rigidbody rb; // Reference to the Rigidbody component
    private Vector3 originalPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Start the countdown for the explosion
        originalPosition = rb.transform.position;
        StartCoroutine(ExplodeAfterDelay());
        // Make the grenade float
        FloatBeforeExplosion();
    }

    void FloatBeforeExplosion()
    {
        // Apply an upward force to make the grenade float
        rb.AddForce(Vector3.up * floatHeight, ForceMode.Impulse);
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

        // Find all colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        // If there are enemies in range, deactivate the Rigidbody and ascend
        bool enemyInRange = false;
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.CompareTag("Enemy")) // Check if the object is an enemy
            {
                enemyInRange = true;
                break;
            }
        }

        if (enemyInRange)
        {
            foreach (Collider nearbyObject in colliders)
            {
                if (nearbyObject.CompareTag("Enemy")) // Check if the object is an enemy
                {
                    // Start the gravitational attraction
                    StartCoroutine(Attract(nearbyObject.transform));

                    // Set isGrenadeEffectActive to true for the affected enemy
                    EnemyAI enemyAI = nearbyObject.GetComponent<EnemyAI>();
                    if (enemyAI != null)
                    {
                        enemyAI.isGrenadeEffectActive = true;
                    }
                }
            }

            // Deactivate the Rigidbody so the grenade doesn't move
            rb.isKinematic = true;
            // Ascend the grenade 2 units
            StartCoroutine(Ascend());

            // Set isGrenadeEffectActive back to false for the affected enemies after the grenade effect has ended
            StartCoroutine(ResetGrenadeEffect(colliders));
        }

        // Destroy the grenade after the explosion
        Destroy(gameObject, attractionDuration + 1f);
    }

    IEnumerator ResetGrenadeEffect(Collider[] affectedEnemies)
    {
        yield return new WaitForSeconds(attractionDuration);

        foreach (Collider enemy in affectedEnemies)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.isGrenadeEffectActive = false;
            }
        }
    }


    IEnumerator Ascend()
    {
        Vector3 startPosition = rb.transform.position;
        Vector3 endPosition = new Vector3(startPosition.x, startPosition.y + floatHeight, startPosition.z);
        float startTime = Time.time;

        while (Time.time < startTime + floatDuration)
        {
            // Interpolate the position on the Y-axis over the floatDuration
            float t = (Time.time - startTime) / floatDuration;
            rb.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }
    }

    IEnumerator Attract(Transform target)
    {
        float elapsedTime = 0f;

        while (elapsedTime < attractionDuration)
        {
            elapsedTime += Time.deltaTime;
            // Use the attractionStrength variable to determine the force applied
            float strength = Mathf.Lerp(attractionStrength, 0f, elapsedTime / attractionDuration);
            Vector3 direction = (transform.position - target.position).normalized;
            target.position += direction * strength * Time.deltaTime;
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
