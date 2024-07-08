using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] CharacterController characterController;
    
    [SerializeField] int speed;
    [SerializeField] int gravity;

    Vector3 movementDirection;
    Vector3 playerVelocity;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(characterController.isGrounded)
        {
            playerVelocity = Vector3.zero;
        }

        movementDirection = Input.GetAxis("Vertical") * transform.forward +
                            Input.GetAxis("Horizontal") * transform.right;

        characterController.Move(movementDirection * speed * Time.deltaTime);

        characterController.Move(playerVelocity * Time.deltaTime);
        playerVelocity.y -= gravity;

    }
}
