using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollection : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string itemName = gameObject.name; // Use the name of the game object as its item name
            GameManager.instance.CollectItem(itemName);
            Destroy(gameObject); // Remove the item after collection
        }
    }
}