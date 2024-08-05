using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(BoxCollider))]
public class GravityGrenade : MonoBehaviour
{
    [SerializeField] float explosionDelay;
    [SerializeField] float explosionRadius;
    [SerializeField] float attractionDuration;
    [SerializeField] float attractionStrength;
    [SerializeField] float floatHeight;
    [SerializeField] float floatDuration;
    [SerializeField] float floatLerpDuration = .5f;

    [SerializeField] AudioSource aud;
    [SerializeField] ParticleSystem pullEffect;
    [SerializeField] AudioClip pullSound;
    [SerializeField] GameObject explosionEffectAndSound;
    
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

    void FloatBeforeExplosion()
    {
        rb.isKinematic = false;
        rb.useGravity = false;
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, gameObject.transform.position + (Vector3.up * floatHeight), floatLerpDuration * Time.deltaTime);
        gameObject.transform.rotation = Quaternion.identity;
        ParticleSystem pullParticle = Instantiate(pullEffect, gameObject.transform.position, Quaternion.identity);
        
        pullParticle.gameObject.transform.localScale.Set(1 / gameObject.transform.localScale.x, 1 / gameObject.transform.localScale.y, 1 / gameObject.transform.localScale.z);
        aud.PlayOneShot(pullSound);
        Destroy(pullParticle, attractionDuration);
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

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && hasExploded)
        {
            Vector3 direction = (transform.position - other.transform.position).normalized;
            float strength = attractionStrength * Time.deltaTime;
            other.transform.position += direction * strength;
        }
    }
}
