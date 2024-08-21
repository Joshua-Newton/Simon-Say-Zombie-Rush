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
    [Range(0, 20)] [SerializeField] float breakVolume = 3.0f;

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

    public void TakeDamage(int amount, string damageSource = "")
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

    public void Stun(float duration)
    {
        // Breakable objects don't need to handle stuns, so this can be left empty or log a message
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
        sphereCollider.enabled = false;
        Destroy(gameObject, 0.5f);
    }

}
