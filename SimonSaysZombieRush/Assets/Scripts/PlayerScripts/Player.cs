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
    [SerializeField] private List<GameObject> grenadeInventory; // List to store different grenade prefabs
    [SerializeField] private Transform grenadeSpawnPoint; // Punto de origen (no se usará en este caso)
    [SerializeField] private float throwForce = 15f; // Fuerza de lanzamiento
    [SerializeField] public int maxGrenades = 2; // Máximo número de granadas que el jugador puede tener
    [SerializeField] public float grenadeCooldown = 5f; // Tiempo de recarga entre granadas

    [Header("----- Healing -----")]
    [SerializeField] int healAmount;
    [SerializeField] float healInterval;
    [SerializeField] float healDelay;

    [Header("----- Other -----")]
    [SerializeField] float immunityAfterDamageFromSameSource = 0.1f;

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

    bool isShooting;
    bool isWallRunning;
    bool isSprinting;
    bool isPlayingStep;
    bool isMeleeing;
    private bool isStunned = false;
    public CameraController cameraController;
    private int currentGrenades;
    private int selectedGrenadeIndex;
    private bool isRecharging;

    private float grenadeSwitchHoldTime = 2f; // Time required to hold the button to switch grenades
    private float grenadeSwitchTimer = 0f; // Timer to track the hold duration

    bool canHeal;
    private List<string> recentDamageSource = new List<string>();

    Collider wallRunCollider;
    Collider meleeCollider;
    Coroutine healingCoroutine;
    Coroutine healingDelayCoroutine;
    #endregion

    #region Unity Methods
    void Start()
    {
        HPOriginal = HP;
        gunModelOriginal = gunModel;
        isRecharging = false;
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
    }

    void Update()
    {
        WeaponMovement();
        if (raySource != null && rayDestination != null)
        {
            Debug.DrawRay(raySource, rayDestination);
        }
        Grounded();
        Movement();
        Sprint();
        Shooting();
        HandleGrenadeSwitch(); // Handle grenade switching with hold

        if (Input.GetButtonDown("Grenade") && currentGrenades > 0)
        {
            ThrowGrenade();
        }
        if (!GameManager.instance.isPaused)
        {
            SelectWeapon();
        }
        Heal();
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

    private void HandleGrenadeSwitch()
    {
        if (Input.GetButton("Grenade"))
        {
            grenadeSwitchTimer += Time.deltaTime;
            if (grenadeSwitchTimer >= grenadeSwitchHoldTime)
            {
                SelectNextGrenade();
                grenadeSwitchTimer = 0f; // Reset the timer after switching
            }
        }
        else
        {
            grenadeSwitchTimer = 0f; // Reset the timer if the button is released early
        }
    }

    private void SelectNextGrenade()
    {
        if (grenadeInventory.Count > 1)
        {
            selectedGrenadeIndex = (selectedGrenadeIndex + 1) % grenadeInventory.Count;
            Debug.Log("Selected Grenade: " + grenadeInventory[selectedGrenadeIndex].name);
        }
    }

    private void SelectPreviousGrenade()
    {
        if (grenadeInventory.Count > 1)
        {
            selectedGrenadeIndex--;
            if (selectedGrenadeIndex < 0) selectedGrenadeIndex = grenadeInventory.Count - 1;
            Debug.Log("Selected Grenade: " + grenadeInventory[selectedGrenadeIndex].name);
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

    private void ThrowGrenade()
    {
        if (grenadeInventory.Count == 0) return;

        GameObject grenadePrefab = grenadeInventory[selectedGrenadeIndex];
        Vector3 targetPoint = transform.position + transform.forward * 5f; // Throws the grenade forward

        GameObject grenade = Instantiate(grenadePrefab, grenadeSpawnPoint.position, Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);

        currentGrenades--;
        StartCoroutine(RechargeGrenade());
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

    private IEnumerator RechargeGrenade()
    {
        isRecharging = true;
        while (currentGrenades < maxGrenades)
        {
            yield return new WaitForSeconds(grenadeCooldown);
            currentGrenades++;
            Debug.Log("Grenades recharged to: " + currentGrenades);

            UpdatePlayerUI(); // Asegúrate de actualizar la UI después de cada recarga
        }
        isRecharging = false;
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
            StartCoroutine(MuzzleFlash());

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

    IEnumerator ReloadGrenade()
    {
        isRecharging = true;
        yield return new WaitForSeconds(grenadeCooldown);
        currentGrenades++;
        isRecharging = false;

        if (currentGrenades < maxGrenades)
        {
            StartCoroutine(RechargeGrenade());
        }
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

    public void AddGrenade(GameObject grenadePrefab)
    {
        if (!grenadeInventory.Contains(grenadePrefab))
        {
            grenadeInventory.Add(grenadePrefab);
            currentGrenades = maxGrenades; // Refill grenades when a new type is picked up
            selectedGrenadeIndex = grenadeInventory.Count - 1; // Automatically switch to the new grenade type
        }
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
            cameraController.TriggerShake(0.5f, 0.3f);
        }

        if (HP <= 0)
        {
            GameManager.instance.LoseGame("You died!");
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
        speed /= slowVariable;
    }

    public void SlowAreaExit(int slowVariable)
    {
        speed *= slowVariable;
    }

    public int GetCurrentGrenades()
    {
        return currentGrenades;
    }

    public int GetMaxGrenades()
    {
        return maxGrenades;
    }

    public void UseGrenade()
    {
        if (currentGrenades > 0 && !isRecharging)
        {
            currentGrenades--;
            Debug.Log("Grenades remaining: " + currentGrenades);

            if (currentGrenades < maxGrenades)
            {
                StartCoroutine(RechargeGrenade());
            }
        }
    }

    public void WeaponMovement()
    {
        if (animator != null)
        {
            if (movementDirection.magnitude == 0)
            {
                animator.SetTrigger("Idle");
                return;
            }

            float angle = Vector3.SignedAngle(transform.forward, movementDirection, Vector3.up);
            if (angle <= 45 && angle >= -45)
            {
                animator.SetTrigger("Move Forward");
            }
            else if (angle <= 135 && angle >= 45)
            {
                animator.SetTrigger("Move Left");
            }
            else if (angle <= -45 && angle >= -135)
            {
                animator.SetTrigger("Move Right");
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
