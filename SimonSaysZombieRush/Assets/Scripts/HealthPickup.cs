using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : PickupBase
{
    [SerializeField] int healthAmount;

    private void OnTriggerEnter(Collider other)
    {
        // comparing tag
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.instance.playerScript.ChangeHP(healthAmount);
        }
       
    }

}
