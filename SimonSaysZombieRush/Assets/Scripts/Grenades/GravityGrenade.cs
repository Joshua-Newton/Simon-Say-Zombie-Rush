using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(BoxCollider))]
public class GravityGrenade : MonoBehaviour
{
    [SerializeField] public float explosionDelay;
    [SerializeField] public float explosionRadius;
    [SerializeField] public float attractionDuration;
    [SerializeField] public float attractionStrength;
    [SerializeField] public float floatHeight;
    [SerializeField] public float floatLerpDuration = .5f;

    [SerializeField] AudioSource aud;
    [SerializeField] ParticleSystem pullEffect;
    [SerializeField] AudioClip pullSound;
    [SerializeField] GameObject explosionEffectAndSound;

    public bool hasExploded = false;
    public Rigidbody rb;
    public SphereCollider sphereCollider;

    private ParticleSystem pullParticleInstance;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        sphereCollider.radius = explosionRadius;
        sphereCollider.enabled = false;
        StartCoroutine(ExplodeAfterDelay());
    }

    void FloatBeforeExplosion()
    {
        // Stop the grenade's movement
        rb.velocity = Vector3.zero;
        rb.isKinematic = true; // Make the grenade kinematic to stop physics-based movement
        rb.useGravity = false;

        // Move the grenade to the floating height over time
        StartCoroutine(FloatToPosition(transform.position + Vector3.up * floatHeight));

        // Instantiate and play the particle effect
        pullParticleInstance = Instantiate(pullEffect, transform.position, Quaternion.identity);
        pullParticleInstance.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
        aud.PlayOneShot(pullSound);

        // Start coroutine to destroy the particle effect after the attraction duration
        StartCoroutine(DestroyPullParticleAfterDuration());
    }

    IEnumerator FloatToPosition(Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 startingPosition = transform.position;

        while (elapsedTime < floatLerpDuration)
        {
            transform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / floatLerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        FloatBeforeExplosion();
        Explode();
    }

    void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;
        sphereCollider.enabled = true;
        StartCoroutine(DisableSphereColliderAfterDuration());
    }

    IEnumerator DisableSphereColliderAfterDuration()
    {
        yield return new WaitForSeconds(attractionDuration);
        sphereCollider.enabled = false;
        Destroy(Instantiate(explosionEffectAndSound), 2f);
        Destroy(gameObject);
    }

    IEnumerator DestroyPullParticleAfterDuration()
    {
        yield return new WaitForSeconds(attractionDuration);

        // Destroy the particle effect after the attraction duration
        if (pullParticleInstance != null)
        {
            pullParticleInstance.Stop(); // Stop the particle effect
            Destroy(pullParticleInstance.gameObject, pullParticleInstance.main.duration); // Wait for the particles to die out and then destroy the GameObject
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && hasExploded)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, other.transform.position);
            if (distanceToEnemy <= explosionRadius)
            {
                Vector3 direction = (transform.position - other.transform.position).normalized;
                float strength = attractionStrength * Time.deltaTime;
                other.transform.position += direction * strength;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position to represent the explosion radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
