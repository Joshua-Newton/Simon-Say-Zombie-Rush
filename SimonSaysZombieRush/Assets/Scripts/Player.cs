using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamage, IJumpPad
{
    [SerializeField] CharacterController characterController;
    
    [SerializeField] int speed;
    [SerializeField] int sprintMultiplier;
    [SerializeField] float gravity;
    [SerializeField] float jumpStrength;
    [SerializeField] int maxJumps;
    [SerializeField] int shootDamage;
    [SerializeField] float shootDelay;
    [SerializeField] float shootRange;
    [SerializeField] int HP;
    [SerializeField] int grappleSpeed;
    [SerializeField] int grappleRange;
    [SerializeField] int grappleMaxConsecutiveUses;
    [SerializeField] LayerMask ignoreLayer;
    
    Vector3 movementDirection;
    Vector3 grappleDirection;
    Vector3 grappleHitPoint;
    Vector3 playerVelocity;
    int numJumps;
    bool isShooting;
    bool isGrappling;
    int HPOriginal;
    int numGrapples;

    // Start is called before the first frame update
    void Start()
    {
        HPOriginal = HP;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Grounded();
        Jump();
        Sprint();
        Shooting();
        GrappleHook();
    }

    void Movement()
    {
        movementDirection = Input.GetAxis("Vertical") * transform.forward +
                            Input.GetAxis("Horizontal") * transform.right;

        characterController.Move(movementDirection * speed * Time.deltaTime);

    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && numJumps < maxJumps && !isGrappling)
        {
            ++numJumps;
            playerVelocity.y = jumpStrength;
        }

        if(!isGrappling)
        {
            characterController.Move(playerVelocity * Time.deltaTime);
            playerVelocity.y -= gravity * Time.deltaTime;
        }
    }

    void Sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMultiplier;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMultiplier;
        }
    }

    void Shooting()
    {
        if(Input.GetButton("Fire1") && !isShooting && !GameManager.instance.isPaused)
        {
            StartCoroutine(Shoot());
        }
    }

    void GrappleHook()
    {
        // Use GetButtonDown and GetButtonUp in conjunction with a bool to set grapple target, but allow player to look away while grappling
        // Then use the bool assigned to actually move the player. If they let go of the grapple button, they stop grappling
        if (Input.GetButtonDown("Fire2") && numGrapples < grappleMaxConsecutiveUses)
        {
            RaycastHit hit;
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, grappleRange, ~ignoreLayer))
            {
                ++numGrapples;
                grappleHitPoint = hit.point;
                grappleDirection = grappleHitPoint - transform.position;
                isGrappling = true;
            }
        }
        else if (Input.GetButtonUp("Fire2"))
        {
            isGrappling = false;
        }

        if(isGrappling)
        {
            grappleDirection = grappleHitPoint - transform.position;
            characterController.Move(grappleDirection * grappleSpeed * Time.deltaTime);
        }

    }

    void Grounded()
    {
        if (characterController.isGrounded)
        {
            numJumps = 0;
            numGrapples = 0;
            playerVelocity = Vector3.zero;
        }
    }

    IEnumerator Shoot()
    {
        isShooting = true;

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootRange, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);
            IDamage damageTarget = hit.collider.GetComponent<IDamage>();
            if (hit.transform != transform && damageTarget != null)
            {
                damageTarget.TakeDamage(shootDamage);            
            }
        }

        yield return new WaitForSeconds(shootDelay);
        isShooting = false;

    }

    public void TakeDamage(int amount)
    {
        HP -= amount;
        if(HP <= 0)
        {
            GameManager.instance.LoseGame();
        }
    }

    public void JumpPad(float jumpPadStrength)
    {
        ++numJumps; // TODO: Design question, should a jump pad count as the player's first jump? I assumed yes - Josh N.
        playerVelocity.y = jumpPadStrength;
    }
}
