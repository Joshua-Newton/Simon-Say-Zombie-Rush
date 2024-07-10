using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] float jumpStrength;

    private void OnTriggerEnter(Collider other)
    {
        IJumpPad jumpPadTarget = other.GetComponent<IJumpPad>();
        if(jumpPadTarget != null )
        {
            jumpPadTarget.JumpPad(jumpStrength);
        }
    }
}
