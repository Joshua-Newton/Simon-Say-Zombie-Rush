using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlowArea : MonoBehaviour
{
    [SerializeField] int slowVariable;

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            { return; }
        ISlowArea slowArea = other.GetComponent<ISlowArea>();
        if (slowArea != null )
        {
            slowArea.SlowArea(slowVariable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ISlowArea slowAreaExit = other.GetComponent<ISlowArea>();
        if (slowAreaExit != null)
        {
            slowAreaExit.SlowAreaExit(slowVariable);
        }
    }

}
