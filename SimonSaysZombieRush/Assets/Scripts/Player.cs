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
    [SerializeField] LayerMask ignoreLayer;
    
    Vector3 movementDirection;
    Vector3 playerVelocity;
    int numJumps;
    bool isShooting;
    int HPOriginal;


    // Start is called before the first frame update
    void Start()
    {
        HPOriginal = HP;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Jump();
        Sprint();
        Shooting();
    }

    void Movement()
    {
        movementDirection = Input.GetAxis("Vertical") * transform.forward +
                            Input.GetAxis("Horizontal") * transform.right;

        characterController.Move(movementDirection * speed * Time.deltaTime);

    }

    void Jump()
    {
        if (characterController.isGrounded)
        {
            numJumps = 0;
            playerVelocity = Vector3.zero;
        }

        if (Input.GetButtonDown("Jump") && numJumps < maxJumps)
        {
            ++numJumps;
            playerVelocity.y = jumpStrength;
        }

        characterController.Move(playerVelocity * Time.deltaTime);
        playerVelocity.y -= gravity * Time.deltaTime;
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
