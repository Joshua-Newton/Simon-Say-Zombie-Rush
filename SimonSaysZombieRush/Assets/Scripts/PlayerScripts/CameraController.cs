using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(1, 100)] [SerializeField] float distanceToPlayer = 10f;
    [Range(0, 90)] [SerializeField] float angleToPlayer = 75f;

    GameObject player;
    Transform initialTransform;
    float initialZOffset;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.instance.player;
        Quaternion newRotation = Quaternion.identity;
        newRotation = Quaternion.Euler(newRotation.x + angleToPlayer, newRotation.y, newRotation.z);
        transform.SetPositionAndRotation(player.transform.position, newRotation);
        transform.position -= transform.TransformDirection(Vector3.forward) * distanceToPlayer;
        
        initialTransform = transform;
        initialZOffset = transform.position.z - player.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        transform.SetPositionAndRotation(new Vector3(player.transform.position.x,
            transform.position.y,
            player.transform.position.z + initialZOffset),
            initialTransform.rotation);
    }
}
