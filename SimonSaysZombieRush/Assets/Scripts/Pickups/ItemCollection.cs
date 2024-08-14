using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCollection : PickupBase
{
    [SerializeField] Sprite itemSprite;
    [Range(5,600)] [SerializeField] int secondsToRetrieve;

    int timerIndex;

    public Sprite GetItemSprite()
    {
        return itemSprite;
    }

    private void Start()
    {

    }

    protected override void Update()
    {
        base.Update();
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
            gameObject.SetActive(false); // Remove the item after collection. However, they still need to exist in lists for the game manager.
        }
    }

    public int GetSecondsToRetrieve() { return secondsToRetrieve; }

    public void SetTimerIndex(int timerIndex) { this.timerIndex = timerIndex; }
    public int GetTimerIndex() { return timerIndex;}
}