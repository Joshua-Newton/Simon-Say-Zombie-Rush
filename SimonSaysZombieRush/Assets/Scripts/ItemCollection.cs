using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollection : MonoBehaviour
{
    [SerializeField] private string itemName; // The name of this item

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.CollectItem(itemName);
            Destroy(gameObject); // Remove the item after collection
        }
    }
}
