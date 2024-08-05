using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    GameObject player;
    Transform initialTransform;
    float initialZOffset;

    // Start is called before the first frame update
    void Start()
    {
        initialTransform = transform;
        player = GameManager.instance.player;
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
