using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBase : MonoBehaviour
{
    [SerializeField] float spinSpeed;

    protected virtual void Update()
    {
        RotatePickup();
    }

    protected void RotatePickup()
    {
        Quaternion rotation = Quaternion.LookRotation(transform.right);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, spinSpeed * Time.deltaTime);
    }
}