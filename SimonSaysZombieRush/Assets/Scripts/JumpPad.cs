using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] float jumpStrength;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTERED JUMP PAD");
        IJumpPad jumpPadTarget = other.GetComponent<IJumpPad>();
        if(jumpPadTarget != null )
        {
            Debug.Log("JUMP PAD NOT NULL");

            jumpPadTarget.JumpPad(jumpStrength);
        }
    }
}
