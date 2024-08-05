using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCollection : PickupBase
{
    [SerializeField] Sprite itemSprite;

    public Sprite GetItemSprite()
    {
        return itemSprite;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string itemName = gameObject.name; // Use the name of the game object as its item name
            if (TimeTrialModeManager.instance)
            {
                TimeTrialModeManager.instance.CollectItem(gameObject);
            }
            else if(HordeModeManager.instance)
            {
                HordeModeManager.instance.CollectItem(gameObject);
            }
            Destroy(gameObject); // Remove the item after collection
        }
    }
}