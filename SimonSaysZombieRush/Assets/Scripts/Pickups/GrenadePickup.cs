using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadePickup : PickupBase
{
    [SerializeField] private GameObject grenadePrefab; // The grenade prefab to be added to the player's inventory
    [SerializeField] private int grenadeAmount = 1;    // The number of grenades to add to the player's inventory

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player playerScript = other.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.AddGrenade(grenadePrefab, grenadeAmount); // Adds the specified number of grenades to the player's inventory
                Destroy(gameObject); // Destroy the grenade pickup after being collected
            }
        }
    }
}
