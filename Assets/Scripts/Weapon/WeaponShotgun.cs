using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;


public class WeaponShotgun : MonoBehaviour, IWeapon
{
    public Camera camera;
    public int damage = 10; // Damage per pellet
    public int pelletsPerShot = 8; // Number of pellets per shot
    public float fireRate = 1f; // Time between shots in seconds

    private float nextFire;

    public enum FireMode { Single, Auto, Burst }
    public FireMode currentFireMode = FireMode.Single; // Start with Single Fire Mode

    [Header("Available Fire Modes")]
    public List<FireMode> availableFireModes = new List<FireMode> { FireMode.Single }; // Default available mode is Single

    [Header("Burst Fire")]
    public int burstCount = 3; // Number of shots in a burst
    public float burstFireRate = 0.1f; // Time between burst shots

    [Header("Ammo")]
    public int chamberSize = 7; // Max shells the shotgun can hold
    public int currentChamberAmmo = 0; // Shells currently loaded in the shotgun
    public int totalAmmo = 40; // Total available slugs

    [Header("Reloading")]
    public float reloadTimePerSlug = 0.321f; // Time to load one shell
    private bool isReloading = false;

    [Header("Spread")]
    public float maxSpreadAngle = 5f; // Maximum spread angle for pellets

    [Header("VFX")]
    public GameObject hitVFX;
    public GameObject muzzleFlash;
    public Transform muzzleTransform; // The location of the muzzle flash

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI totalAmmoText;
    public TextMeshProUGUI fireModeText; // UI to display current fire mode

    [Header("Animation")]
    public Animator animator; // Animator reference
    private string isReloadingBool = "isReloading"; // Boolean to control the reloading state
    private string reloadTrigger = "reloadTrigger"; // Trigger for each shell load animation
    private string fireTrigger = "fireTrigger"; // Trigger for the Fire animation

    private Coroutine burstCoroutine; // To handle burst fire

    [Header("ADS Settings")]
    public GameObject adsObject; // The object you want to move during ADS (e.g., the weapon model)
    public bool isAiming = false; // Tracks if player is aiming
    public float adsFOV = 40f; // Field of view while aiming
    public float normalFOV = 60f; // Normal field of view
    public Vector3 adsPosition = new Vector3(0, -0.2f, 0.5f); // Position of the gun while aiming
    public Vector3 normalPosition = new Vector3(0.5f, -0.5f, 1.0f); // Normal position of the gun
    public float adsSpeed = 0.2f; // Speed of transitioning between ADS and normal

    void Start()
    {
        currentChamberAmmo = chamberSize; // Initialize the shotgun with full ammo
        UpdateWeaponUI();
        UpdateFireModeUI(); // Update fire mode UI on start
    }

    void Update()
    {
        // Prevent firing while reloading
        isReloading = animator.GetCurrentAnimatorStateInfo(0).IsName("reload");

        // Fire the shotgun based on current fire mode
        if (currentFireMode == FireMode.Single)
        {
            HandleSingleShotFire();
        }
        else if (currentFireMode == FireMode.Burst)
        {
            HandleBurstFire();
        }

        // Switch fire mode when pressing 'B'
        if (Input.GetKeyDown(KeyCode.B))
        {
            SwitchFireMode();
        }

        // Reload the shotgun one shell at a time
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentChamberAmmo < chamberSize && totalAmmo > 0)
        {
            StartCoroutine(ReloadShotgun());
        }

        // Handle the cooldown between shots
        if (nextFire > 0)
        {
            nextFire -= Time.deltaTime;
        }

        // Handle ADS logic
        HandleADS();
    }

    void HandleADS()
    {
        if (Input.GetMouseButtonDown(1)) // Right-click to toggle ADS
        {
            isAiming = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
        }

        // Smoothly transition the camera's FOV and the weapon's position
        float smoothSpeed = Time.deltaTime * adsSpeed; // Adjusted smooth speed calculation

        // Check if adsObject is assigned, if not, fallback to this weapon's transform
        Transform targetTransform = adsObject != null ? adsObject.transform : transform;

        if (isAiming)
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, adsFOV, smoothSpeed);

            // Move the specified adsObject during ADS
            targetTransform.localPosition = Vector3.Lerp(targetTransform.localPosition, adsPosition, smoothSpeed);
        }
        else
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, normalFOV, smoothSpeed);

            // Return the target to the normal position
            targetTransform.localPosition = Vector3.Lerp(targetTransform.localPosition, normalPosition, smoothSpeed);
        }
    }

    public void HandleSingleShotFire()
    {
        if (Input.GetButtonDown("Fire1") && nextFire <= 0 && currentChamberAmmo > 0 && !isReloading)
        {
            FireShotgun();
            nextFire = fireRate;
        }
    }

    public void HandleBurstFire()
    {
        if (Input.GetButtonDown("Fire1") && burstCoroutine == null && currentChamberAmmo > 0 && totalAmmo > 0 && !isReloading)
        {
            burstCoroutine = StartCoroutine(FireBurst());
        }
    }

    IEnumerator FireBurst()
    {
        int shotsFired = 0;

        while (shotsFired < burstCount && currentChamberAmmo > 0)
        {
            nextFire = burstFireRate; // Delay between burst shots

            FireShotgun();
            shotsFired++;

            yield return new WaitForSeconds(burstFireRate);
        }

        burstCoroutine = null; // Reset burst coroutine
    }

    void FireShotgun()
    {
        // Trigger the Fire animation
        animator.SetTrigger(fireTrigger);

        // Muzzle flash effect
        GameObject flashInstance = Instantiate(muzzleFlash, muzzleTransform.position, muzzleTransform.rotation); 
        Destroy(flashInstance, 0.05f); // Muzzle flash lasts for a brief moment

        // Fire multiple pellets (simulating shotgun spread)
        for (int i = 0; i < pelletsPerShot; i++)
        {
            // Calculate random spread
            Vector3 fireDirection = camera.transform.forward;
            fireDirection.x += Random.Range(-maxSpreadAngle, maxSpreadAngle) * 0.01f;
            fireDirection.y += Random.Range(-maxSpreadAngle, maxSpreadAngle) * 0.01f;

            Ray ray = new Ray(camera.transform.position, fireDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                GameObject impact = Instantiate(hitVFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 0.5f);

                // Apply damage to any hit objects
                Health targetHealth = hit.transform.gameObject.GetComponent<Health>();
                if (targetHealth != null)
                {
                    // Calculate remaining health after this pellet's damage
                    int remainingHealth = targetHealth.health - damage;

                    // Apply the damage across the network using RPC
                    hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);

                    // Check if the damage results in the target's death
                    if (remainingHealth <= 0)
                    {
                        RoomManager.instance.kills++;
                        RoomManager.instance.SetHashes();
                    }
                }
            }
        }
        // Reduce current chamber ammo after firing
        currentChamberAmmo--;
        UpdateWeaponUI();
    }

    IEnumerator ReloadShotgun()
    {
        isReloading = true;
        animator.SetBool(isReloadingBool, true); // Start the reload process with "Prior to Reload" animation

        // Wait for "Prior to Reload" animation to complete
        yield return new WaitForSeconds(0.2f);

        while (currentChamberAmmo < chamberSize && totalAmmo > 0)
        {
            // Play the reload animation for each shell being loaded
            animator.SetTrigger(reloadTrigger); // Trigger the reload animation per shell

            // Wait for the "ReloadOne" animation to complete
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("ReloadOne"));
            yield return new WaitForSeconds(reloadTimePerSlug); // Time taken to load each shell

            // Add one shell to the shotgun chamber
            currentChamberAmmo++;
            totalAmmo--;

            UpdateWeaponUI();
        }

        isReloading = false;
        animator.SetBool(isReloadingBool, false); // End the reload process, return to idle
    }

    // Switch to the next available fire mode when pressing 'B'
    public void SwitchFireMode()
    {
        // Find the index of the current fire mode in the available modes list
        int currentIndex = availableFireModes.IndexOf(currentFireMode);

        // Move to the next fire mode in the list, or wrap around to the first one
        currentIndex = (currentIndex + 1) % availableFireModes.Count;

        // Set the current fire mode to the new mode
        currentFireMode = availableFireModes[currentIndex];

        // Update the UI to reflect the current fire mode
        UpdateFireModeUI();
    }

    // Update the fire mode text UI
    public void UpdateFireModeUI()
    {
        if (fireModeText != null)
        {
            fireModeText.text = currentFireMode.ToString(); // Display the current fire mode as text
        }
    }
    
    // Updates the UI to show the current chamber ammo and total ammo
    public void UpdateWeaponUI()
    {
        ammoText.text = currentChamberAmmo + " / " + chamberSize;
        totalAmmoText.text = totalAmmo.ToString();
        fireModeText.text = currentFireMode.ToString(); // Display the current fire mode as text
    }
}
