using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseReturnZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && TimeTrialModeManager.instance != null)
        {
            TimeTrialModeManager.instance.ReturnToBase();
        }
    }
}
