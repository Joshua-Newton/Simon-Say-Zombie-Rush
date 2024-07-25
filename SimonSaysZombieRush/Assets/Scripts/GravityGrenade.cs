using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider), typeof(BoxCollider))]
public class GravityGrenade : MonoBehaviour
{
    [SerializeField] private float explosionDelay;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float attractionDuration;
    [SerializeField] private float attractionStrength;
    [SerializeField] private float floatHeight;
    [SerializeField] private float floatDuration;

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
        rb.useGravity = false;
        rb.AddForce(Vector3.up * floatHeight, ForceMode.Impulse);
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
        Destroy(gameObject, 1f);
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
