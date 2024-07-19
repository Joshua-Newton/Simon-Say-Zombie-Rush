using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] WeaponStats weapon;

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
