using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObjects : MonoBehaviour, IDamage
{
    [Header("----- Stats -----")]
    [SerializeField] int HP;
    [SerializeField] Renderer model;
    [SerializeField] Color hitColor;
    [SerializeField] MeshRenderer barrel;

    [Header("----- Exploding Object -----")]
    [SerializeField] int explosionDamage;
    [SerializeField] GameObject explosionPrefab;

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
            //soundSource.PlayOneShot(breakSound, breakVolume);
            GameObject explosion = Instantiate(explosionPrefab, gameObject.transform.position, Quaternion.identity);
            explosion.GetComponent<Explosion>().SetExplosionDamage(explosionDamage);
            Destroy(gameObject);
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

}
