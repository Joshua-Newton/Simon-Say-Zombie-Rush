using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponPickup : PickupBase
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

    public WeaponStats GetWeaponStats()
    {
        return weapon;
    }
}
