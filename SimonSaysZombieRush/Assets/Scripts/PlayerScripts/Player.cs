using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour, IDamage, IJumpPad, ISlowArea
{
    #region Serialized Fields
    [Header("----- Components -----")]
    [SerializeField] CharacterController characterController;
    [SerializeField] AudioSource aud;
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] Animator animator;
    private Rigidbody playerRigidbody;

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
    [SerializeField] Transform shootPos;
    [SerializeField] int damage;
    [SerializeField] float damageRange;
    [SerializeField] float damageDelay;

    [Header("----- Grenade -----")]
    [SerializeField] private GameObject gravityGrenadePrefab; // Prefab de la granada
    [SerializeField] private Transform grenadeSpawnPoint; // Punto de origen (no se usará en este caso)
    [SerializeField] private float throwForce = 15f; // Fuerza de lanzamiento
    [SerializeField] private float raycastDistance = 100f; // Distancia máxima del raycast
    [SerializeField] private int maxGrenades = 2; // Máximo número de granadas que el jugador puede tener
    [SerializeField] private float grenadeCooldown = 5f; // Tiempo de recarga entre granadas

    [Header("----- Healing -----")]
    [SerializeField] int healAmount; // Amount to heal per tick
    [SerializeField] float healInterval; // Interval between each healing tick
    [SerializeField] float healDelay; // Time after taking damage before healing
    #endregion

    #region Private fields
    GameObject gunModelOriginal;
    GameObject meleeModelOriginal;

    Vector3 movementDirection;
    Vector3 playerVelocity;

    int HPOriginal;
    int selectedWeapon;
    int originalSpeed;
    int slowedSpeed;

    bool isShooting;
    bool isGrappling;
    bool isWallRunning;
    bool isSprinting;
    bool isPlayingStep;
    bool isMeleeing;
    bool isBurning;
    bool isSlowed;
    private bool isStunned = false;
    public CameraController cameraController;
    private int currentGrenades; // Número actual de granadas disponibles
    private bool isReloading = false; // Indica si se está recargando una granada

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
        currentGrenades = maxGrenades; // Iniciar con la cantidad máxima de granadas
        meleeModelOriginal = meleeModel;
        originalSpeed = speed;
        EquipStartingWeapons();
        UpdatePlayerUI();
        SpawnPlayer();
        cameraController = Camera.main.GetComponent<CameraController>();
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
        // Llamar a ThrowGrenade solo si hay granadas disponibles y no se está recargando
        if (Input.GetButtonDown("Grenade") && currentGrenades > 0 && !isReloading)
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
        foreach(GameObject weapon in startingWeapons)
        {
            WeaponPickup pickup = weapon.GetComponent<WeaponPickup>();
            if (pickup != null)
            {
                GetWeaponStats(pickup.GetWeaponStats());
            }
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

    void ThrowGrenade()
    {
        // Realiza un raycast desde la cámara hacia la posición del mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            // Si el raycast golpea algo, obtiene la posición de impacto
            targetPoint = hit.point;
        }
        else
        {
            // Si no golpea nada, calcula una posición en el espacio
            targetPoint = ray.GetPoint(raycastDistance);
        }

        // Instancia la granada en la posición objetivo
        GameObject grenade = Instantiate(gravityGrenadePrefab, targetPoint, Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        // Aplica una fuerza hacia abajo para simular la caída
        rb.AddForce(Vector3.down * throwForce, ForceMode.VelocityChange);

        currentGrenades--; // Decrementa el número de granadas
        StartCoroutine(ReloadGrenade()); // Inicia la recarga de la granada
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
        else if (currentWeapon.weaponType == WeaponStats.WeaponType.Projectile)
        {
            isShooting = true;
            aud.PlayOneShot(currentWeapon.shootSound, currentWeapon.shootVol);

            currentWeapon.ammoCurrent--;
            UpdatePlayerUI();

            Instantiate(currentWeapon.projectile, shootPos.position, shootPos.rotation);
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

    IEnumerator ReloadGrenade()
    {
        isReloading = true; // Marca como que está recargando
        yield return new WaitForSeconds(grenadeCooldown); // Espera el tiempo de recarga
        currentGrenades++; // Recarga una granada
        isReloading = false; // Marca que ha terminado la recarga
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

        // Trigger camera shake based on damage
        if (cameraController != null)
        {
            // Set shake intensity proportional to damage, with clamping to ensure reasonable values
            float shakeMagnitude = Mathf.Clamp((float)amount / 100f, 0.1f, 1f);
            cameraController.TriggerShake(0.5f, shakeMagnitude); // Shake for 0.5 seconds with proportional intensity

        }

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

    public void SlowArea(int slowVariable)
    {
        speed /= slowVariable;
        isSlowed = true;
    }

    public void SlowAreaExit(int slowVariable)
    {
        speed *= slowVariable;
        isSlowed = false;
    }

    #endregion

 

 
}
