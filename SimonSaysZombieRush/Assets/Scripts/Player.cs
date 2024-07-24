using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour, IDamage, IJumpPad
{
    #region Serialized Fields
    [Header("----- Components -----")]
    [SerializeField] CharacterController characterController;
    [SerializeField] AudioSource aud;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] Animator animator;

    [Header("----- Sounds -----")]
    [SerializeField] AudioClip[] audioSteps;
    [Range(0, 1)] [SerializeField] float audioStepsVolume = 0.5f;
    [SerializeField] AudioClip[] audioJumps;
    [Range(0, 1)] [SerializeField] float audioJumpsVolume = 0.5f;
    [SerializeField] AudioClip[] audioHurt;
    [Range(0, 1)] [SerializeField] float audioHurtVolume = 0.5f;
    [SerializeField] AudioClip[] audioMelee;
    [Range(0, 1)][SerializeField] float audioMeleeVolume = 0.5f;


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
    [SerializeField] GameObject gunModel;
    [SerializeField] GameObject meleeModel;

    [SerializeField] int damage;
    [SerializeField] float damageRange;
    [SerializeField] float damageDelay;

    [Header("----- Grapple -----")]
    [SerializeField] int grappleSpeed;
    [SerializeField] int grappleRange;
    [SerializeField] int grappleMaxConsecutiveUses;
    [SerializeField] LineRenderer grappleRenderer;

    [Header("----- Wall Run -----")]
    [SerializeField] int wallRunSpeed;
    [SerializeField] float maxWallRunTime;
    [Range(0, 3)] [SerializeField] float wallRunDistance;

    [Header("----- Grenade -----")]
    [SerializeField] GameObject gravityGrenadePrefab; // Prefab of the gravity grenade
    [SerializeField] Transform grenadeSpawnPoint; // Spawn point to throw the grenade from
    [SerializeField] float throwForce = 15f; // Throwing force of the grenade
    [SerializeField] float raycastDistance = 100f; // Maximum distance for the raycast

    [Header("----- Healing -----")]
    [SerializeField] int healAmount; // Amount to heal per tick
    [SerializeField] float healInterval; // Interval between each healing tick
    [SerializeField] float healDelay; // Time after taking damage before healing
    #endregion

    #region Private fields
    GameObject gunModelOriginal;
    GameObject meleeModelOriginal;

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
    bool isSprinting;
    bool isPlayingStep;
    bool isMeleeing;

    bool canHeal;
    bool canWallRun = true;

    float initialWallRunAngle;
    float heightOriginal;
    float origPosY;
    float origScaleY;

    Collider wallRunCollider;
    Coroutine healingCoroutine;
    Coroutine healingDelayCoroutine;

    LastWallRun lastWallRun;

    RaycastHit leftHit;
    RaycastHit rightHit;
    #endregion

    #region enum definitions
    enum LastWallRun { none, left, right };

    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        HPOriginal = HP;
        heightOriginal = characterController.height;
        origPosY = transform.position.y;
        origScaleY = transform.localScale.y;
        gunModelOriginal = gunModel;
        meleeModelOriginal = meleeModel;
        UpdatePlayerUI();
        SpawnPlayer();
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
        Heal();
    }
    #endregion

    #region Private Methods
    void Movement()
    {

        movementDirection = Input.GetAxis("Vertical") * transform.forward +
                            Input.GetAxis("Horizontal") * transform.right;

        characterController.Move(movementDirection * speed * Time.deltaTime);

        if ((characterController.isGrounded && movementDirection.magnitude > 0.2f && !isPlayingStep) ||
            !characterController.isGrounded && movementDirection.magnitude > 0.2f && isWallRunning && !isPlayingStep)
        {
            StartCoroutine(PlayStep());
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && numJumps < maxJumps && !isGrappling)
        {
            aud.PlayOneShot(audioJumps[Random.Range(0, audioJumps.Length)], audioJumpsVolume);
            ++numJumps;
            if (isWallRunning)
            {
                if (lastWallRun == LastWallRun.left)
                {
                    Vector3 playerJumpVector = new Vector3(0, jumpStrength, 0);
                    Vector3 jumpVelocity = (leftHit.normal.normalized + playerJumpVector.normalized) * jumpStrength;
                    playerVelocity = jumpVelocity;
                }
                else if (lastWallRun == LastWallRun.right)
                {
                    Vector3 playerJumpVector = new Vector3(0, jumpStrength, 0);
                    Vector3 jumpVelocity = (rightHit.normal.normalized + playerJumpVector.normalized) * jumpStrength;
                    playerVelocity = jumpVelocity;
                }
                AbruptEndWallRun();
            }
            else
            {
                playerVelocity.y = jumpStrength;
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
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMultiplier;
            isSprinting = false;
        }
    }

    void Shooting()
    {
        if (Input.GetButton("Fire1") && weaponList.Count > 0 && weaponList[selectedWeapon].weaponType == WeaponStats.WeaponType.Gun && weaponList[selectedWeapon].ammoCurrent > 0 && !isShooting && !GameManager.instance.isPaused)
        {
            StartCoroutine(Shoot());
        }
        else if (Input.GetButton("Fire1") && weaponList.Count > 0 && weaponList[selectedWeapon].weaponType == WeaponStats.WeaponType.Melee && !isMeleeing && !GameManager.instance.isPaused)
        {
            Melee();
        }
    }

    void WallRun()
    {
        if(!isWallRunning && characterController.isGrounded)
        {
            lastWallRun = LastWallRun.none;
        }

        bool wallOnLeft = Physics.Raycast(Camera.main.transform.position, -Camera.main.transform.right, out leftHit, wallRunDistance, ~ignoreLayer);
        bool wallOnRight = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.right, out rightHit, wallRunDistance, ~ignoreLayer);
        Debug.DrawRay(Camera.main.transform.position, -Camera.main.transform.right);
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.right);

        if (isWallRunning && !wallOnLeft && !wallOnRight)
        {
            AbruptEndWallRun();
        }
        
        // If there's a wall on the left... Or we jumped off a wall run on the right
        if((!isWallRunning && wallOnLeft && leftHit.collider.CompareTag("WallRun")) && (canWallRun || lastWallRun != LastWallRun.left))
        {
            lastWallRun = LastWallRun.left;
            InitiateWallRun();
        }
        // If there's a wall on the right... Or we jumped off a wall run on the left
        if ((!isWallRunning && wallOnRight && rightHit.collider.CompareTag("WallRun")) && (canWallRun || lastWallRun != LastWallRun.right))
        {
            lastWallRun = LastWallRun.right;
            InitiateWallRun();
        }
    }

    void GrappleHook()
    {
        // Use GetButtonDown and GetButtonUp in conjunction with a bool to set grapple target, but allow player to look away while grappling
        // Then use the bool assigned to actually move the player. If they let go of the grapple button, they stop grappling
        if (Input.GetButtonDown("Fire2") && !GameManager.instance.isPaused && numGrapples < grappleMaxConsecutiveUses)
        {
            RaycastHit hit;
            if (!GameManager.instance.isPaused && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, grappleRange, ~ignoreLayer))
            {
                grappleRenderer.enabled = true;
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
            grappleRenderer.SetPosition(0, transform.position);
            grappleRenderer.SetPosition(1, grappleHitPoint);
            grappleDirection = (grappleHitPoint - transform.position).normalized;
            characterController.Move(grappleDirection * grappleSpeed * Time.deltaTime);
        }
        else
        {
            grappleRenderer.enabled = false;
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
            canWallRun = true;
            numJumps = 0;
            numGrapples = 0;
            playerVelocity = Vector3.zero;
        }
    }

    void EndWallRun()
    {
        isWallRunning = false;
        canWallRun = false;
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

    void Heal()
    {
        if (canHeal && HP < HPOriginal)
        {
            healingCoroutine = StartCoroutine(HealHealth());
        }
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
        UpdatePlayerUI();

        damage = weaponList[selectedWeapon].damage;
        damageRange = weaponList[selectedWeapon].damageRange;

        if (weaponList[selectedWeapon].weaponType == WeaponStats.WeaponType.Gun)
        {
            gunModel.SetActive(true);
            meleeModel.SetActive(false);
            gunModel.GetComponent<MeshFilter>().sharedMesh = weaponList[selectedWeapon].weaponModel.GetComponent<MeshFilter>().sharedMesh;
            gunModel.GetComponent<MeshRenderer>().sharedMaterials = weaponList[selectedWeapon].weaponModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
        else if (weaponList[selectedWeapon].weaponType == WeaponStats.WeaponType.Melee)
        {
            gunModel.SetActive(false);
            meleeModel.SetActive(true);
            meleeModel.GetComponent<MeshFilter>().sharedMesh = weaponList[selectedWeapon].weaponModel.GetComponent<MeshFilter>().sharedMesh;
            meleeModel.GetComponent<MeshRenderer>().sharedMaterials = weaponList[selectedWeapon].weaponModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
    }

    void Melee()
    {
        WeaponStats currentWeapon = weaponList[selectedWeapon];
        if (currentWeapon.weaponType == WeaponStats.WeaponType.Melee)
        {
            isMeleeing = true;
            aud.PlayOneShot(audioMelee[Random.Range(0, audioMelee.Length)], audioMeleeVolume);
            animator.SetTrigger("Melee");
        }
    }

    #endregion

    #region IEnumerator Coroutines

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

    IEnumerator PlayStep()
    {
        isPlayingStep = true;

        aud.PlayOneShot(audioSteps[Random.Range(0, audioSteps.Length)], audioStepsVolume);

        if (!isSprinting)
        {
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        isPlayingStep = false;
    }

    IEnumerator HealHealth()
    {
        canHeal = false;
        HP += healAmount;
        if (HP > HPOriginal)
        {
            HP = HPOriginal;
        }
        UpdatePlayerUI();
        yield return new WaitForSeconds(healInterval);

        if (HP < HPOriginal)
        {
            canHeal = true;
        }

    }

    IEnumerator HealDelay()
    {
        yield return new WaitForSeconds(healDelay);
        canHeal = true;
    }

    IEnumerator Shoot()
    {
        WeaponStats currentWeapon = weaponList[selectedWeapon];
        if (currentWeapon.weaponType == WeaponStats.WeaponType.Gun)
        {
            isShooting = true;
            aud.PlayOneShot(currentWeapon.shootSound, currentWeapon.shootVol);

            currentWeapon.ammoCurrent--;
            UpdatePlayerUI();

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, damageRange, ~ignoreLayer))
            {
                IDamage damageTarget = hit.collider.GetComponent<IDamage>();
                if (hit.transform != transform && damageTarget != null)
                {
                    damageTarget.TakeDamage(damage);
                }
                else
                {
                    Instantiate(weaponList[selectedWeapon].hitEffect, hit.point, Quaternion.identity);
                }
            }
        }
        
        
        yield return new WaitForSeconds(damageDelay);
        isShooting = false;
    }


    #endregion

    #region Public Functions

    public void ChangeHP(int HealthAmount)
    {
        HP += HealthAmount;
        if (HP > HPOriginal)
        {
            HP = HPOriginal;
        }
        UpdatePlayerUI();
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
        aud.PlayOneShot(audioHurt[Random.Range(0, audioHurt.Length)], audioHurtVolume);
        StartCoroutine(flashScreenDamage());

        if (HP <= 0)
        {
            GameManager.instance.LoseGame();
        }
        else
        {
            // The player has taken damage, so we can heal that damage. 
            // Reset heal delay and stop healing, then start a delay that once it has passed we can start healing
            if(healingDelayCoroutine != null)
            {
                StopCoroutine(healingDelayCoroutine);
            }
            if(healingCoroutine != null)
            {
                StopCoroutine(healingCoroutine);
            }
            canHeal = false;
            healingDelayCoroutine = StartCoroutine(HealDelay());
        }
    }

    public void JumpPad(float jumpPadStrength)
    {
        ++numJumps; // TODO: Design question, should a jump pad count as the player's first jump? I assumed yes - Josh N.
        playerVelocity.y = jumpPadStrength;
    }

    public void AbruptEndWallRun()
    {
        StopCoroutine(WallRunTimer());
        EndWallRun();
    }

    public void UpdatePlayerUI()
    {
        GameManager.instance.playerHPBar.fillAmount = (float)HP / HPOriginal;
        if (weaponList.Count > 0)
        {   
            GameManager.instance.ammoCurrent.text = weaponList[selectedWeapon].ammoCurrent.ToString("F0");
            GameManager.instance.ammoMax.text = weaponList[selectedWeapon].ammoMax.ToString("F0");
        }
    }

    public void InitiateWallRun(Collider wallTrigger)
    {
        wallRunCollider = wallTrigger;
        isWallRunning = true;
        numJumps = 0; // Reset jumps so that the player can jump off the wall, otherwise they might run out of jumps while wall running
        initialWallRunAngle = Vector3.Angle(Camera.main.transform.forward, wallRunCollider.transform.forward);
        StartCoroutine(WallRunTimer());
    }

    public void InitiateWallRun()
    {
        playerVelocity = Vector3.zero;
        isWallRunning = true;
        numJumps = 0; // Reset jumps so that the player can jump off the wall, otherwise they might run out of jumps while wall running
        StartCoroutine(WallRunTimer());
    }

    public void GetWeaponStats(WeaponStats weapon)
    {
        if (weaponList.Contains(weapon))
        {
            return;
        }
        else
        {
            weaponList.Add(weapon);        
        }
        selectedWeapon = weaponList.Count - 1;

        UpdatePlayerUI();

        damage = weapon.damage;
        damageRange = weapon.damageRange;
        damageDelay = weapon.damageDelay;

        if(weapon.weaponType == WeaponStats.WeaponType.Gun)
        {
            gunModel.SetActive(true);
            meleeModel.SetActive(false);
            gunModel.GetComponent<MeshFilter>().sharedMesh = weapon.weaponModel.GetComponent<MeshFilter>().sharedMesh;
            gunModel.GetComponent<MeshRenderer>().sharedMaterials = weapon.weaponModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
        else if(weapon.weaponType == WeaponStats.WeaponType.Melee)
        {
            gunModel.SetActive(false);
            meleeModel.SetActive(true);
            meleeModel.GetComponent<MeshFilter>().sharedMesh = weapon.weaponModel.GetComponent<MeshFilter>().sharedMesh;
            meleeModel.GetComponent<MeshRenderer>().sharedMaterials = weapon.weaponModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
    }

    public void MeleeOff()
    {
        if(weaponList.Count > 0 && weaponList[selectedWeapon].weaponModel != null)
        {
            Collider meleeCollider = weaponList[selectedWeapon].weaponModel.GetComponent<Collider>();
            if(meleeCollider != null)
            {
                meleeCollider.enabled = false;
            }
        }
        isMeleeing = false;
    }

    public void MeleeOn()
    {
        Collider meleeCollider = weaponList[selectedWeapon].weaponModel.GetComponent<Collider>();
        if (meleeCollider != null)
        {
            meleeCollider.enabled = true;
        }
    }


    #endregion
}
