using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("----- Explosion Sound -----")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] explosionSounds;
    [Range(0, 1)] [SerializeField] float volume;

    [Header("----- Explosion Components -----")]
    [SerializeField] ParticleSystem particles;
    [SerializeField] SphereCollider explosionCollider;

    [Header("----- Explosion Stats -----")]
    [SerializeField] float explosionRadiusMax;
    [SerializeField] bool expandingExplosion;
    [SerializeField] float explosionStartingRadius;
    [SerializeField] float explosionDamageTime;
    [SerializeField] float explosionObjectLifeTime;

    // Start is called before the first frame update
    protected void Start()
    {
        if (expandingExplosion)
        {
            explosionCollider.radius = Mathf.Lerp(explosionStartingRadius, explosionRadiusMax, explosionDamageTime);
        }
        else
        {
            explosionCollider.radius = explosionRadiusMax;
        }
        audioSource.PlayOneShot(explosionSounds[Random.Range(0, explosionSounds.Length)], volume);
        StartCoroutine(EnableDamage());
        Destroy(gameObject, explosionObjectLifeTime);
    }

    protected IEnumerator EnableDamage()
    {
        explosionCollider.isTrigger = true;
        explosionCollider.enabled = true;
        yield return new WaitForSeconds(explosionDamageTime);
        explosionCollider.enabled = false;
    }

    public void SetExplosionDamage(int newDamage)
    {
        explosionCollider.GetComponent<Damage>().SetDamage(newDamage);
    }

}
