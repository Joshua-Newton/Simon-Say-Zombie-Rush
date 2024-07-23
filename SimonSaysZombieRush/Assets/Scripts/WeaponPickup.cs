using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] WeaponStats weapon;
    [SerializeField] float spinSpeed;

    private void Update()
    {
        Quaternion rotation = Quaternion.LookRotation(-transform.right);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, spinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            weapon.ammoCurrent = weapon.ammoMax;
            GameManager.instance.playerScript.GetWeaponStats(weapon);
            Destroy(gameObject);
        }
    }
}
