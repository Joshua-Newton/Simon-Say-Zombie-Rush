using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObjects : MonoBehaviour, IDamage
{
    [Header("----- Stats -----")]
    [SerializeField] int HP;
    [SerializeField] Renderer model;
    [SerializeField] Color hitColor;
    [SerializeField] GameObject explosion;
    [SerializeField] MeshRenderer barrel;

    [Header("----- Sounds -----")]
    [SerializeField] AudioSource soundSource;
    [SerializeField] AudioClip breakSound;
    [Range(0, 5)] [SerializeField] float breakVolume = 3.0f;

    [Header("----- Exploding Object -----")]
    [SerializeField] int explosionDamage;
    [SerializeField] ParticleSystem explosionEffect;

    [SerializeField] SphereCollider sphereCollider;
    [SerializeField] float explodeLength;
    
    float explosionDist;

    Vector3 origin;
    Vector3 direction;

    Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = model.material.color;
    }

    public void TakeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(FlashDamage());
        if (HP < 0)
        {
            soundSource.PlayOneShot(breakSound, breakVolume);
            Instantiate(explosionEffect, gameObject.transform.position, Quaternion.identity);
            StartCoroutine(Explode());
        }
    }

    IEnumerator FlashDamage()
    {
        model.material.color = hitColor;
        yield return new WaitForSeconds(.1f);
        model.material.color = originalColor;
    }

    IEnumerator Explode()
    {
        sphereCollider.enabled = true;
        barrel.enabled = false;
        yield return new WaitForSeconds(explodeLength);
        Destroy(gameObject);
    }

}
