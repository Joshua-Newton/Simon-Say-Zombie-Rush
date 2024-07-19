using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour, IDamage, IJumpPad
{
    [Header("----- Components -----")]
    [SerializeField] CharacterController characterController;
    [SerializeField] AudioSource aud;
    [SerializeField] LayerMask ignoreLayer;

    [Header("----- Player -----")]
    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMultiplier;

    [Header("----- Jump -----")]
    [SerializeField] float gravity;
    [SerializeField] float jumpStrength;
    [SerializeField] int maxJumps;

    [Header("----- Weapons -----")]
    [SerializeField] List<WeaponStats> weaponList = new List<WeaponStats>();
    [SerializeField] GameObject weaponModel;
    [SerializeField] int damage;
    [SerializeField] float damageRange;
    [SerializeField] float damageDelay;

    [Header("----- Grapple -----")]
    [SerializeField] int grappleSpeed;
    [SerializeField] int grappleRange;
    [SerializeField] int grappleMaxConsecutiveUses;

    [Header("----- Wall Run -----")]
    [SerializeField] int wallRunSpeed;
    [SerializeField] float maxWallRunTime;

    [Header("----- Grenade -----")]
    public GameObject gravityGrenadePrefab; // Prefab of the gravity grenade
    public Transform grenadeSpawnPoint; // Spawn point to throw the grenade from
    [SerializeField] float throwForce = 15f; // Throwing force of the grenade
    [SerializeField] float raycastDistance = 100f; // Maximum distance for the raycast

    [Header("----- Healing -----")]
    [SerializeField] int healAmount; // Amount to heal per tick
    [SerializeField] float healInterval; // Interval between each healing tick
    [SerializeField] int healDuration; // Total duration for the healing effect

    Vector3 movementDirection;
    Vector3 grappleDirection;
    Vector3 grappleHitPoint;
    Vector3 playerVelocity;

    int numJumps;
    int HPOriginal;
    int numGrapples;
    int selectedWeapon;

    bool isShooting;
    bool isGrappling;
    bool isWallRunning;
    bool isCrouching;

    float initialWallRunAngle;
    float heightOriginal;
    float origPosY;
    float origScaleY;

    Collider wallRunCollider;
    Coroutine healingCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        HPOriginal = HP;
        heightOriginal = characterController.height;
        origPosY = transform.position.y;
        origScaleY = transform.localScale.y;
        UpdatePlayerUI();
    }

    // Update is called once per frame
    void Update()
    {
        Jump();
        Movement();
        Sprint();
        Shooting();
        GrappleHook();
        WallRun();
        ThrowGrenade();
        Crouch();
        if (!GameManager.instance.isPaused)
        {
            SelectWeapon();
        }
    }

    void Movement()
    {
        if (!isWallRunning)
        {
            movementDirection = Input.GetAxis("Vertical") * transform.forward +
                                Input.GetAxis("Horizontal") * transform.right;

            characterController.Move(movementDirection * speed * Time.deltaTime);
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && numJumps < maxJumps && !isGrappling)
        {
            ++numJumps;
            playerVelocity.y = jumpStrength;
            if (isWallRunning)
            {
                AbruptEndWallRun();
            }
        }

        if (!isGrappling && !isWallRunning)
        {
            characterController.Move(playerVelocity * Time.deltaTime);
            playerVelocity.y -= gravity * Time.deltaTime;
        }

        // Grounded() has to come immediately after jump in order to properly check characterController.isGrounded
        Grounded();
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
        if (Input.GetButton("Fire1") && weaponList.Count > 0 && !isShooting && !GameManager.instance.isPaused)
        {
            StartCoroutine(Shoot());
        }
    }

    void WallRun()
    {
        if (isWallRunning)
        {
            float angle = Vector3.Angle(Camera.main.transform.forward, wallRunCollider.transform.forward);

            float input = Input.GetAxis("Vertical");
            if (input <= 0)
            {
                AbruptEndWallRun();
            }
            else if (angle < 90 && initialWallRunAngle < 90)
            {
                movementDirection = input * wallRunCollider.transform.forward * Time.deltaTime;
            }
            else if (angle >= 90 && initialWallRunAngle >= 90)
            {
                movementDirection = input * -wallRunCollider.transform.forward * Time.deltaTime;
            }
            else
            {
                // Player changed look direction, so stop wall running
                AbruptEndWallRun();
            }
            characterController.Move(movementDirection * wallRunSpeed);
        }
        else
        {
            wallRunCollider = null;
        }
    }

    void GrappleHook()
    {
        // Use GetButtonDown and GetButtonUp in conjunction with a bool to set grapple target, but allow player to look away while grappling
        // Then use the bool assigned to actually move the player. If they let go of the grapple button, they stop grappling
        if (Input.GetButtonDown("Fire2") && numGrapples < grappleMaxConsecutiveUses)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, grappleRange, ~ignoreLayer))
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

        if (isGrappling)
        {
            grappleDirection = (grappleHitPoint - transform.position).normalized;
            characterController.Move(grappleDirection * grappleSpeed * Time.deltaTime);
        }
    }

    void ThrowGrenade()
    {
        if (Input.GetButtonDown("Grenade")) // Assign a button to throw the grenade (e.g., E button)
        {
            Ray ray = new Ray(grenadeSpawnPoint.position, grenadeSpawnPoint.forward);
            RaycastHit hit;

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit, raycastDistance))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(raycastDistance);
            }

            // Instantiate the grenade at the spawn point
            GameObject grenade = Instantiate(gravityGrenadePrefab, grenadeSpawnPoint.position, grenadeSpawnPoint.rotation);
            Rigidbody rb = grenade.GetComponent<Rigidbody>();

            // Calculate the direction to the target point
            Vector3 direction = (targetPoint - grenadeSpawnPoint.position).normalized;

            // Apply force to throw the grenade towards the target point
            rb.AddForce(direction * throwForce, ForceMode.VelocityChange);
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

        aud.PlayOneShot(weaponList[selectedWeapon].shootSound, weaponList[selectedWeapon].shootVol);
        weaponList[selectedWeapon].ammoCurrent--;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, damageRange, ~ignoreLayer))
        {
            IDamage damageTarget = hit.collider.GetComponent<IDamage>();
            if (hit.transform != transform && damageTarget != null)
            {
                damageTarget.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(damageDelay);
        isShooting = false;
    }

    public void SpawnPlayer()
    {
        HP = HPOriginal;
        UpdatePlayerUI();
        characterController.enabled = false;
        transform.position = GameManager.instance.playerSpawnPos.transform.position;
        characterController.enabled = true;
    }

    public void TakeDamage(int amount)
    {
        HP -= amount;
        UpdatePlayerUI();
        StartCoroutine(flashScreenDamage());

        if (HP <= 0)
        {
            GameManager.instance.LoseGame();
        }
        else
        {
            // Start healing over time after taking damage
            if (healingCoroutine != null)
            {
                StopCoroutine(healingCoroutine);
            }
            healingCoroutine = StartCoroutine(HealOverTime());
        }
    }

    public void JumpPad(float jumpPadStrength)
    {
        ++numJumps; // TODO: Design question, should a jump pad count as the player's first jump? I assumed yes - Josh N.
        playerVelocity.y = jumpPadStrength;
    }

    IEnumerator flashScreenDamage()
    {
        GameManager.instance.dmgFlashBckgrnd.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        GameManager.instance.dmgFlashBckgrnd.SetActive(false);
    }

    IEnumerator WallRunTimer()
    {
        yield return new WaitForSeconds(maxWallRunTime);
        EndWallRun();
    }

    public void AbruptEndWallRun()
    {
        StopCoroutine(WallRunTimer());
        EndWallRun();
    }

    public void UpdatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)HP / HPOriginal;
    }

    public void InitiateWallRun(Collider wallTrigger)
    {
        wallRunCollider = wallTrigger;
        isWallRunning = true;
        numJumps = 0; // Reset jumps so that the player can jump off the wall, otherwise they might run out of jumps while wall running
        initialWallRunAngle = Vector3.Angle(Camera.main.transform.forward, wallRunCollider.transform.forward);
        StartCoroutine(WallRunTimer());
    }

    void EndWallRun()
    {
        isWallRunning = false;
        wallRunCollider = null;
        initialWallRunAngle = 0;
    }

    void Crouch()
    {
        if (Input.GetButtonDown("Crouch"))
        {
            characterController.height = heightOriginal / 2;
            transform.localScale = new Vector3(transform.localScale.x, origScaleY / 2, transform.localScale.z);
            transform.position = new Vector3(transform.position.x, origPosY / 2, transform.position.z);
            isCrouching = true;
            
        }
        else if (Input.GetButtonUp("Crouch") && isCrouching)
        {
            characterController.height = heightOriginal;
            transform.localScale = new Vector3(transform.localScale.x, origScaleY, transform.localScale.z);
            transform.position = new Vector3(transform.position.x, origPosY, transform.position.z);
            isCrouching = false;
        }


    }

    // Coroutine for healing over time
    IEnumerator HealOverTime()
    {
        float elapsedTime = 0;

        while (elapsedTime < healDuration)
        {
            HP += healAmount;
            if (HP > HPOriginal)
            {
                HP = HPOriginal;
                UpdatePlayerUI();
                yield break;
            }
            UpdatePlayerUI();
            yield return new WaitForSeconds(healInterval);
            elapsedTime += healInterval;
        }
    }

    public void GetWeaponStats(WeaponStats weapon)
    {
        weaponList.Add(weapon);
        selectedWeapon = weaponList.Count - 1;

        damage = weapon.damage;
        damageRange = weapon.damageRange;
        damageDelay = weapon.damageDelay;

        weaponModel.GetComponent<MeshFilter>().sharedMesh = weapon.weaponModel.GetComponent<MeshFilter>().sharedMesh;
        weaponModel.GetComponent<MeshRenderer>().sharedMaterials = weapon.weaponModel.GetComponent<MeshRenderer>().sharedMaterials;
    }

    void SelectWeapon()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && selectedWeapon < weaponList.Count - 1)
        {
            selectedWeapon++;
            ChangeWeapon();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && selectedWeapon > 0)
        {
            selectedWeapon--;
            ChangeWeapon();
        }
    }

    void ChangeWeapon()
    {
        damage = weaponList[selectedWeapon].damage;
        damageRange = weaponList[selectedWeapon].damageRange;
        damageRange = weaponList[selectedWeapon].damageRange;

        weaponModel.GetComponent<MeshFilter>().sharedMesh = weaponList[selectedWeapon].weaponModel.GetComponent<MeshFilter>().sharedMesh;
        weaponModel.GetComponent<MeshRenderer>().sharedMaterials = weaponList[selectedWeapon].weaponModel.GetComponent<MeshRenderer>().sharedMaterials;
    }
}
