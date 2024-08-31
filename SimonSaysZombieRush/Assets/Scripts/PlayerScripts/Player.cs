using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamage, IJumpPad, ISlowArea
{
    #region Serialized Fields
    [Header("----- Components -----")]
    [SerializeField] CharacterController characterController;
    [SerializeField] AudioSource aud;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] Animator animator;
    [SerializeField] int animSpeedTransition;
    [SerializeField] AudioSource menuButtonClickSource;

    [Header("----- Sounds -----")]
    [SerializeField] AudioClip[] audioSteps;
    [Range(0, 1)][SerializeField] float audioStepsVolume = 0.5f;
    [SerializeField] AudioClip[] audioHurt;
    [Range(0, 1)][SerializeField] float audioHurtVolume = 0.5f;
    [SerializeField] AudioClip[] audioMelee;
    [Range(0, 1)][SerializeField] float audioMeleeVolume = 0.5f;
    [SerializeField] AudioClip audioDamage;
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
    [SerializeField] GameObject rpgModel;
    [SerializeField] GameObject muzzleFlash;
    [SerializeField] Transform shootPos;
    [SerializeField] int damage;
    [SerializeField] float damageRange;
    [SerializeField] float damageDelay;

    [Header("----- Grenade -----")]
    [SerializeField] private List<GameObject> grenadeTypes; // List to store different grenade prefabs
    [SerializeField] private Transform grenadeSpawnPoint; // Grenade spawn point
    [SerializeField] private float throwForce = 15f; // Throw force
    [SerializeField] private int maxGrenadesPerType = 2; // Maximum number of grenades of each type the player can hold

    [Header("----- Healing -----")]
    [SerializeField] int healAmount;
    [SerializeField] float healInterval;
    [SerializeField] float healDelay;

    [Header("----- Other -----")]
    [SerializeField] float immunityAfterDamageFromSameSource = 0.1f;
    [SerializeField] GameObject objectivePointer;

    #endregion

    #region Private fields
    GameObject gunModelOriginal;
    GameObject meleeModelOriginal;
    GameObject rpgModelOriginal;

    Vector3 movementDirection;
    Vector3 playerVelocity;

    int HPOriginal;
    int selectedWeapon;
    int originalSpeed;
    int slowedSpeed;
    int numSlowAreas;

    bool isShooting;
    bool isSprinting;
    bool isPlayingStep;
    bool isMeleeing;
    private bool isStunned = false;
    public CameraController cameraController;
    private bool isThrowingGrenade;
    private bool allItemsFound;

    bool canHeal;
    private List<string> recentDamageSource = new List<string>();

    Collider meleeCollider;
    Coroutine healingCoroutine;
    Coroutine healingDelayCoroutine;

    GameObject currentUrgentObjective;

    // Grenade inventory to track quantities for each grenade type
    private Dictionary<GameObject, int> grenadeInventory = new Dictionary<GameObject, int>();
    private int selectedGrenadeIndex = 0;

    #endregion

    #region Unity Methods
    void Start()
    {
        HPOriginal = HP;
        gunModelOriginal = gunModel;
        meleeModelOriginal = meleeModel;
        rpgModelOriginal = rpgModel;
        originalSpeed = speed;
        EquipStartingWeapons();
        UpdatePlayerUI();
        SpawnPlayer();
        cameraController = Camera.main.GetComponent<CameraController>();
        meleeCollider = meleeModel.GetComponent<Collider>();
        if (meleeCollider != null)
        {
            meleeCollider.enabled = false;
        }

        // Initialize grenade inventory with 0 grenades for each type
        foreach (GameObject grenade in grenadeTypes)
        {
            grenadeInventory[grenade] = 0;
        }
    }

    void Update()
    {
        if (currentUrgentObjective == null && !allItemsFound)
        {
            UpdateTargetObjective();
        }
        WeaponMovement();
        Grounded();
        Movement();
        Sprint();
        Shooting();

        if (Input.GetButtonDown("Grenade") && !isThrowingGrenade)
        {
            ThrowGrenade();
        }

        if (Input.GetButtonDown("SwitchGrenade"))
        {
            SelectNextGrenade(); // Switch to the next grenade type
        }

        if (!GameManager.instance.isPaused)
        {
            SelectWeapon();
        }
        Heal();
        UpdateObjectivePointer();
    }
    #endregion

    #region Private Methods
    void EquipStartingWeapons()
    {
        foreach (GameObject weapon in startingWeapons)
        {
            WeaponPickup pickup = weapon.GetComponent<WeaponPickup>();
            if (pickup != null)
            {
                GetWeaponStats(pickup.GetWeaponStats());
            }
        }
    }

    private void SelectNextGrenade()
    {
        if (grenadeTypes.Count > 1)
        {
            selectedGrenadeIndex = (selectedGrenadeIndex + 1) % grenadeTypes.Count;
        }
    }

    private void SelectPreviousGrenade()
    {
        if (grenadeTypes.Count > 1)
        {
            selectedGrenadeIndex--;
            if (selectedGrenadeIndex < 0) selectedGrenadeIndex = grenadeTypes.Count - 1;
        }
    }

    void Gravity()
    {
        playerVelocity.y -= gravity;
        characterController.Move(playerVelocity);
    }

    void Movement()
    {
        movementDirection = (Input.GetAxis("Horizontal") * Vector3.right + Input.GetAxis("Vertical") * Vector3.forward);
        if (movementDirection.magnitude > 1)
        {
            movementDirection.Normalize();
        }

        characterController.Move(movementDirection * speed * Time.deltaTime);
        Gravity();
        if (characterController.isGrounded && movementDirection.magnitude > 0.2f && !isPlayingStep)
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

        // This fixes a WebGL bug where player could let go of shift when not focused on the game to get a huge speed increase
        if (isSprinting && !Input.GetButton("Sprint"))
        {
            speed /= sprintMultiplier;
            isSprinting = false;
        }
    }

    void Shooting()
    {
        WeaponStats.WeaponType type = weaponList[selectedWeapon].weaponType;
        if (Input.GetButton("Fire1") && weaponList.Count > 0 && type != WeaponStats.WeaponType.Melee && weaponList[selectedWeapon].ammoCurrent > 0 && !isShooting && !GameManager.instance.isPaused)
        {
            StartCoroutine(Shoot());
        }
        else if (Input.GetButton("Fire1") && weaponList.Count > 0 && type == WeaponStats.WeaponType.Melee && !isMeleeing && !GameManager.instance.isPaused)
        {
            Melee();
        }
    }

    private IEnumerator GrenadeThrowCooldown()
    {
        yield return new WaitForSeconds(1f); // Example cooldown time, adjust as needed
        isThrowingGrenade = false;
    }

    void Grounded()
    {
        if (characterController.isGrounded)
        {
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
        if (!Input.GetButton("EnableZoom") && Input.GetAxis("Mouse ScrollWheel") > 0 && selectedWeapon < weaponList.Count - 1)
        {
            selectedWeapon++;
            ChangeWeapon();
        }
        else if (!Input.GetButton("EnableZoom") && Input.GetAxis("Mouse ScrollWheel") < 0 && selectedWeapon > 0)
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

        if (weaponList[selectedWeapon].weaponType == WeaponStats.WeaponType.Gun)
        {
            gunModel.SetActive(true);
            meleeModel.SetActive(false);
            rpgModel.SetActive(false);
            gunModel.GetComponent<MeshFilter>().sharedMesh = weaponList[selectedWeapon].weaponModel.GetComponent<MeshFilter>().sharedMesh;
            gunModel.GetComponent<MeshRenderer>().sharedMaterials = weaponList[selectedWeapon].weaponModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
        else if (weaponList[selectedWeapon].weaponType == WeaponStats.WeaponType.Melee)
        {
            gunModel.SetActive(false);
            meleeModel.SetActive(true);
            rpgModel.SetActive(false);
            meleeModel.GetComponent<MeshFilter>().sharedMesh = weaponList[selectedWeapon].weaponModel.GetComponent<MeshFilter>().sharedMesh;
            meleeModel.GetComponent<MeshRenderer>().sharedMaterials = weaponList[selectedWeapon].weaponModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
        else if (weaponList[selectedWeapon].weaponType == WeaponStats.WeaponType.Projectile)
        {
            gunModel.SetActive(false);
            meleeModel.SetActive(false);
            rpgModel.SetActive(true);
            rpgModel.GetComponent<MeshFilter>().sharedMesh = weaponList[selectedWeapon].weaponModel.GetComponent<MeshFilter>().sharedMesh;
            rpgModel.GetComponent<MeshRenderer>().sharedMaterials = weaponList[selectedWeapon].weaponModel.GetComponent<MeshRenderer>().sharedMaterials;
        }
    }

    void Melee()
    {
        WeaponStats currentWeapon = weaponList[selectedWeapon];
        if (currentWeapon.weaponType == WeaponStats.WeaponType.Melee)
        {
            isMeleeing = true;
            animator.SetTrigger("Melee");
            isMeleeing = false;
        }
    }

    void UpdateObjectivePointer()
    {
        if (currentUrgentObjective != null)
        {
            objectivePointer.transform.LookAt(currentUrgentObjective.transform.position);
            objectivePointer.transform.Rotate(new Vector3(90, 0, 0), Space.Self);
        }
        else
        {
            objectivePointer.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Lava")
        {
            StartCoroutine(PlayBurning());
        }
    }

    void OnTriggerExit(Collider other)
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
            StartCoroutine(MuzzleFlash());

            currentWeapon.ammoCurrent--;
            UpdatePlayerUI();

            Vector3 raySource = GameManager.instance.player.transform.position;
            Vector3 rayDestination = GameManager.instance.player.GetComponent<FaceMouse>().GetCurrentMousePos() - raySource;

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
        else if (currentWeapon.weaponType == WeaponStats.WeaponType.Projectile)
        {
            isShooting = true;
            aud.PlayOneShot(currentWeapon.shootSound, currentWeapon.shootVol);

            currentWeapon.ammoCurrent--;
            UpdatePlayerUI();

            Instantiate(currentWeapon.projectile, shootPos.position, transform.rotation);
        }

        yield return new WaitForSeconds(damageDelay);
        isShooting = false;
    }

    IEnumerator PlayBurning()
    {
        aud.PlayOneShot(audioDamage, damageVolume);
        yield return new WaitForSeconds(0.05f);
    }

    IEnumerator RemoveDamageSourceAfterDelay(string damageSource)
    {
        yield return new WaitForSeconds(immunityAfterDamageFromSameSource);
        recentDamageSource.Remove(damageSource);
    }

    IEnumerator MuzzleFlash()
    {
        GameObject flash = Instantiate(muzzleFlash, shootPos.position, Quaternion.identity);
        yield return new WaitForSeconds(0.02f);
        Destroy(flash);
    }

    #endregion

    #region Public Functions

    public void UpdateTargetObjective()
    {
        if (TimeTrialModeManager.instance != null)
        {
            currentUrgentObjective = TimeTrialModeManager.instance.GetNextActiveObjective();
            if (currentUrgentObjective == null)
            {
                currentUrgentObjective = TimeTrialModeManager.instance.GetBaseReturnZone();
            }
        }
    }

    public void AddGrenade(GameObject grenadePrefab, int quantity)
    {
        if (grenadeInventory.ContainsKey(grenadePrefab))
        {
            grenadeInventory[grenadePrefab] = Mathf.Min(grenadeInventory[grenadePrefab] + quantity, maxGrenadesPerType);
        }
        else
        {
            grenadeInventory[grenadePrefab] = Mathf.Min(quantity, maxGrenadesPerType);
            grenadeTypes.Add(grenadePrefab); // Add to the available grenade types if it's new
        }

        // Auto-select the new grenade type if it's new
        selectedGrenadeIndex = grenadeTypes.IndexOf(grenadePrefab);

        UpdatePlayerUI(); // Update UI to reflect the new grenade count
    }

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

    public void TakeDamage(int amount, string damageSource = "")
    {
        if (!string.IsNullOrEmpty(damageSource) && recentDamageSource.Contains(damageSource))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(damageSource))
        {
            recentDamageSource.Add(damageSource);
            StartCoroutine(RemoveDamageSourceAfterDelay(damageSource));
        }

        HP -= amount;
        UpdatePlayerUI();
        aud.PlayOneShot(audioHurt[Random.Range(0, audioHurt.Length)], audioHurtVolume);
        StartCoroutine(flashScreenDamage());

        if (cameraController != null)
        {
            cameraController.TriggerShake(0.5f, 0.3f); // Customize the shake duration and magnitude
        }

        if (HP <= 0)
        {
            Die();
        }
        else
        {
            if (healingDelayCoroutine != null)
            {
                StopCoroutine(healingDelayCoroutine);
            }
            if (healingCoroutine != null)
            {
                StopCoroutine(healingCoroutine);
            }
            canHeal = false;
            healingDelayCoroutine = StartCoroutine(HealDelay());
        }
    }

    private void Die()
    {
        cameraController.ZeroShakeOffset();
        GameManager.instance.LoseGame("You died!");
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
            GameManager.instance.currWeapon.text = weaponList[selectedWeapon].weaponName;
            GameManager.instance.ammoCurrent.text = weaponList[selectedWeapon].ammoCurrent.ToString("F0");
            GameManager.instance.ammoMax.text = weaponList[selectedWeapon].ammoMax.ToString("F0");
        }

        // Update grenade UI here based on the current selected grenade and its quantity
        // Example:
        // GameManager.instance.grenadeCountText.text = grenadeInventory[grenadeTypes[selectedGrenadeIndex]].ToString();
    }

    private void ThrowGrenade()
    {
        GameObject selectedGrenade = grenadeTypes[selectedGrenadeIndex];

        if (grenadeInventory[selectedGrenade] > 0)
        {
            isThrowingGrenade = true;
            grenadeInventory[selectedGrenade]--; // Decrease the count

            GameObject grenade = Instantiate(selectedGrenade, grenadeSpawnPoint.position, Quaternion.identity);
            Rigidbody rb = grenade.GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);

            UpdatePlayerUI(); // Update UI to reflect the current grenade count

            // Optional: Implement cooldown or delay for next grenade throw
            StartCoroutine(GrenadeThrowCooldown());
        }
    }

    public int GetCurrentGrenades()
    {
        GameObject selectedGrenade = grenadeTypes[selectedGrenadeIndex];
        return grenadeInventory[selectedGrenade];
    }

    public GameObject GetSelectedGrenade()
    {
        return grenadeTypes[selectedGrenadeIndex];
    }

    public int GetMaxGrenades(GameObject grenadePrefab)
    {
        return maxGrenadesPerType;
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
        meleeCollider.enabled = false;
    }

    public void MeleeOn()
    {
        meleeCollider.enabled = true;
    }

    public void PlayMeleeAudio()
    {
        aud.PlayOneShot(audioMelee[Random.Range(0, audioMelee.Length)], audioMeleeVolume);
    }

    public void SlowArea(int slowVariable)
    {
        numSlowAreas++;
        if (numSlowAreas == 1)
        {
            // modify speed if this is the first area that has been entered (i.e. this is the only slow area entered)
            speed /= slowVariable;
        }
    }

    public void SlowAreaExit(int slowVariable)
    {
        numSlowAreas--;
        if (numSlowAreas == 0)
        {
            // modify speed if this is the last area that has been exited (i.e. not in another slow area)
            speed *= slowVariable;
        }
    }

    public void WeaponMovement()
    {
        if (animator != null)
        {
            float angle = Vector3.SignedAngle(transform.forward, movementDirection, Vector3.up);

            if (movementDirection.magnitude == 0 && weaponList[selectedWeapon].weaponType == WeaponStats.WeaponType.Melee)
            {
                animator.SetTrigger("Idle Melee");
                return;
            }
            else if (movementDirection.magnitude == 0 && weaponList[selectedWeapon].weaponType != WeaponStats.WeaponType.Melee)
            {
                animator.SetTrigger("Idle Gun");
                return;
            }
            else if (angle <= 45 && angle >= -45)
            {
                animator.SetTrigger("Move Forward");
            }
            else if (angle <= 135 && angle >= 45)
            {
                animator.SetTrigger("Move Right");
            }
            else if (angle <= -45 && angle >= -135)
            {
                animator.SetTrigger("Move Left");
            }
            else
            {
                animator.SetTrigger("Move Back");
            }
        }
    }

    public void PlayButtonClick()
    {
        menuButtonClickSource.Play();
    }

    #endregion
}
