using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : Damage
{
    [Header("----- Explosion Sound -----")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] explosionSounds;
    [Range(0, 1)][SerializeField] float volume;

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
    protected override void Start()
    {
        base.Start();
        audioSource.PlayOneShot(explosionSounds[Random.Range(0, explosionSounds.Length)], volume);
        if (expandingExplosion)
        {
            explosionCollider.radius = Mathf.Lerp(explosionStartingRadius, explosionRadiusMax, explosionDamageTime);
        }
        else
        {
            explosionCollider.radius = explosionRadiusMax;
        }
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



    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
