using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObjects : MonoBehaviour, IDamage
{
    [SerializeField] int HP;
    [SerializeField] int explosionDamage;
    [SerializeField] Renderer model;
    [SerializeField] Color hitColor;
    [SerializeField] ParticleSystem explosionEffect;

    Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        originalColor = model.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int amount)
    {
        HP -= amount;
        StartCoroutine(FlashDamage());
        if (HP < 0)
        {
            Instantiate(explosionEffect, gameObject.transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    IEnumerator FlashDamage()
    {
        model.material.color = hitColor;
        yield return new WaitForSeconds(.1f);
        model.material.color = originalColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(Explode());
        }
        else if (other.CompareTag("Enemy"))
        {
            StartCoroutine(Explode());
        }
    }

    IEnumerator Explode()
    {
        HP -= explosionDamage;
        yield return new WaitForSeconds(0.1f);
    }

}
