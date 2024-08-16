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
    [SerializeField] AudioClip[] audioHurt;
    [Range(0, 1)] [SerializeField] float audioHurtVolume = 0.5f;
    [SerializeField] AudioClip[] audioMelee;
    [Range(0, 1)][SerializeField] float audioMeleeVolume = 0.5f;
    [SerializeField]  AudioClip audioDamage;
    [Range(0, 10)][SerializeField] float damageVolume;

    [Header("----- Player -----")]
    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMultiplier;

    [Header("----- Gravity -----")]
    [SerializeField] float gravity;

    [Header("----- Weapons -----")]
    [SerializeField] List<GameObject> startingWeapons;
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

    [Header("----- Grenade -----")]
    [SerializeField] private GameObject gravityGrenadePrefab; // Prefab of the gravity grenade
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

    int HPOriginal;
    int numGrapples;
    int selectedWeapon;

    bool isShooting;
    bool isGrappling;
    bool isWallRunning;
    bool isSprinting;
    bool isPlayingStep;
    bool isMeleeing;
    bool isBurning;
    private bool isStunned = false;
    public CameraController cameraController;

    bool canHeal;

    Collider wallRunCollider;
    Coroutine healingCoroutine;
    Coroutine healingDelayCoroutine;

    #endregion

    #region enum definitions

    #endregion

    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        HPOriginal = HP;
        gunModelOriginal = gunModel;
        meleeModelOriginal = meleeModel;
        EquipStartingWeapons();
        UpdatePlayerUI();
        SpawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if(raySource != null && rayDestination != null)
        {
            Debug.DrawRay(raySource, rayDestination);
        }
        Grounded();
        Movement();
        Sprint();
        Shooting();
        GrappleHook();
        ThrowGrenade();
        if (!GameManager.instance.isPaused)
        {
            SelectWeapon();
        }
        Heal();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Trigger the camera shake with a duration of 0.5 seconds and magnitude of 0.5
            cameraController.TriggerShake(0.5f, 0.5f);
        }
    }
    #endregion

    #region Private Methods
    void EquipStartingWeapons()
    {
        foreach(GameObject weapon in startingWeapons)
        {
            WeaponPickup pickup = weapon.GetComponent<WeaponPickup>();
            if (pickup != null)
            {
                GetWeaponStats(pickup.GetWeaponStats());
            }
        }
    }

    void Movement()
    {
        movementDirection = (Input.GetAxis("Horizontal") * Vector3.right + Input.GetAxis("Vertical") * Vector3.forward);
        if (movementDirection.magnitude > 1)
        {
            movementDirection.Normalize();
        }

        characterController.Move(movementDirection * speed * Time.deltaTime);

        if ((characterController.isGrounded && movementDirection.magnitude > 0.2f && !isPlayingStep) ||
            !characterController.isGrounded && movementDirection.magnitude > 0.2f && isWallRunning && !isPlayingStep)
        {
            StartCoroutine(PlayStep());
        }
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
            numGrapples = 0;
            playerVelocity = Vector3.zero;
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
        damageDelay = weaponList[selectedWeapon].damageDelay;

        if (weaponList[selectedWeapon].weaponType == WeaponStats.WeaponType.Gun || weaponList[selectedWeapon].weaponType == WeaponStats.WeaponType.Projectile)
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

    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.tag == "Lava")
        {
            StartCoroutine(PlayBurning());
        }
    }

    void OnTriggerExit (Collider other)
    {
        if (other.gameObject.tag == "Lava")
        {
            StopCoroutine(PlayBurning());
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

    Vector3 raySource;
    Vector3 rayDestination;
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

            raySource = GameManager.instance.player.transform.position;
            rayDestination = GameManager.instance.player.GetComponent<FaceMouse>().GetCurrentMousePos() - raySource;
            
            RaycastHit hit;
            if (Physics.Raycast(raySource, rayDestination, out hit, damageRange, ~ignoreLayer))
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

    IEnumerator PlayBurning()
    {
        isBurning = true;
        aud.PlayOneShot(audioDamage, damageVolume);
        yield return new WaitForSeconds(0.05f);
        isBurning = false;
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
            GameManager.instance.LoseGame("You died!");
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

    public void Stun(float duration)
    {
        if (!isStunned)
        {
            StartCoroutine(StunCoroutine(duration));
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        characterController.enabled = false;
        animator.enabled = false;

        yield return new WaitForSeconds(duration);

        characterController.enabled = true;
        animator.enabled = true;
        isStunned = false;
    }

    public void JumpPad(float jumpPadStrength)
    {
        playerVelocity.y = jumpPadStrength;
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

        ChangeWeapon();
    }

    public void MeleeOff()
    {
        if(weaponList.Count > 0 && weaponList[selectedWeapon].weaponModel != null)
        {
            Collider meleeCollider = meleeModel.GetComponent<Collider>();
            // TODO: Implement melee weapons that have their collider as part of the WeaponStats
            //Collider meleeCollider = weaponList[selectedWeapon].weaponModel.GetComponent<Collider>();
            if(meleeCollider != null)
            {
                meleeCollider.enabled = false;
            }
        }
        isMeleeing = false;
    }

    public void MeleeOn()
    {
        Collider meleeCollider = meleeModel.GetComponent<Collider>();
        // TODO: Implement melee weapons that have their collider as part of the WeaponStats
        //Collider meleeCollider = weaponList[selectedWeapon].weaponModel.GetComponent<Collider>();
        if (meleeCollider != null)
        {
            meleeCollider.enabled = true;
        }
    }

    public void UpdateGrenadePrefab(GameObject newGrenadePrefab)
    {
        gravityGrenadePrefab = newGrenadePrefab;
        Debug.Log("Updated Grenade Prefab: " + gravityGrenadePrefab.name);
    }


    #endregion

 

 
}
