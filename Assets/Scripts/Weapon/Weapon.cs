using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;

public class Weapon : MonoBehaviour, IWeapon
{
    public Camera camera;
    public int damage;
    public float fireRate = 0.3f; // Time between shots in seconds for auto fire
    public WeaponIKHandler ikHandler; // Reference to the new IK handling scrip

    private float nextFire;

    public enum FireMode { Single, Auto, Burst }
    public FireMode currentFireMode = FireMode.Single; // Start with Single Fire Mode

    [Header("Available Fire Modes")]
    public List<FireMode> availableFireModes = new List<FireMode> { FireMode.Single }; // Default available mode is Single

    [Header("Burst Fire")]
    public int burstCount = 3; // Number of shots in a burst
    public float burstFireRate = 0.1f; // Time between burst shots

    [Header("VFX")]
    public GameObject hitVFX;
    public GameObject muzzleFlash;
    public Transform muzzleTransform; // The location of the muzzle flash
    public float muzzleFlashDuration = 0.05f; // Time the muzzle flash is visible

    [Header("Ammo")]
    public int mag = 5;
    public int ammo = 30;
    public int magAmmo = 30;
    public int lowAmmoThreshold = 5; // Threshold for low ammo warning

    [Header("UI")]
    public TextMeshProUGUI magText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI fireModeText; // UI to display current fire mode

    [Header("Animation")]
    public Animator animator; 
    private string reloadTrigger = "reload"; 
    private string recoilTrigger = "recoil"; // Recoil animation trigger
    private bool isReloading = false;

    [Header("Recoil Animation")]
    private bool isFiring = false; // Boolean to control recoil animation

    private bool hasFiredSingleShot = false; // Tracks whether the single-shot weapon has fired in this press
    private Coroutine burstCoroutine; // To handle burst fire

    [Header("ADS Settings")]
    public GameObject adsObject;  // The object you want to move during ADS (e.g., the weapon model)
    public GameObject adsSight;   // The sight object where the player should look through when ADS
    public bool isAiming = false; // Tracks if player is aiming
    public float adsFOV = 40f;    // Field of view while aiming
    public float normalFOV = 60f; // Normal field of view
    public Vector3 adsPosition = new Vector3(0, -0.2f, 0.5f); // Position of the gun while aiming
    public Vector3 normalPosition = new Vector3(0.5f, -0.5f, 1.0f); // Normal position of the gun
    public float adsSpeed = 0.2f; // Speed of transitioning between ADS and normal


    [Header("Recoil Settings")]
    public Vector2[] recoilPattern; // Array of x, y values for fixed recoil pattern
    public float recoilResetTime = 0.5f; // Time after which recoil resets
    private int currentRecoilIndex = 0;
    private float recoilTimer;
    public float recoilRandomFactor = 0.1f; // Adds randomness to recoil

    [Header("Randomized Spread Settings")]
    public float baseSpread = 0.01f; // Base spread angle
    public float maxSpread = 0.1f; // Maximum spread angle
    public float spreadIncreasePerShot = 0.01f; // Spread increase per shot
    public float spreadResetSpeed = 0.05f; // Speed at which spread resets to base
    private float currentSpread;

    [Header("Magazine Settings")]
    public Transform magPosition; // Transform or GameObject representing the magazine



    void Start()
    {
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;

        // Update the UI with the initial fire mode
        UpdateFireModeUI();

        // Initialize recoil and spread
        currentSpread = baseSpread;
        recoilTimer = recoilResetTime;
    }

    void Update()
    {
        // Decrease fire rate cooldown
        if (nextFire > 0)
        {
            nextFire -= Time.deltaTime;
        }

        // Reset recoil over time
        if (recoilTimer > 0)
        {
            recoilTimer -= Time.deltaTime;
        }
        else
        {
            currentRecoilIndex = 0;
        }

        // Reset spread over time
        if (currentSpread > baseSpread)
        {
            currentSpread -= spreadResetSpeed * Time.deltaTime;
        }
        else
        {
            currentSpread = baseSpread;
        }

        // Prevent firing while reloading
        isReloading = animator.GetCurrentAnimatorStateInfo(0).IsName("reload");

        // Switch fire mode when pressing 'B'
        if (Input.GetKeyDown(KeyCode.B))
        {
            SwitchFireMode();
        }

        // Handle Firing Based on Fire Mode
        if (currentFireMode == FireMode.Auto)
        {
            HandleAutomaticFire();
        }
        else if (currentFireMode == FireMode.Single)
        {
            HandleSingleShotFire();
        }
        else if (currentFireMode == FireMode.Burst)
        {
            HandleBurstFire();
        }

        // Handle Reload
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && mag > 0)
        {
            Reload();
        }

        HandleADS(); // Call ADS logic
        HandleLowAmmoWarning(); // Update ammo UI warning when ammo is low
    }
    // Low ammo UI update
    void HandleLowAmmoWarning()
    {
        if (ammo <= lowAmmoThreshold)
        {
            ammoText.color = Color.red; // Set ammo text to red
        }
        else
        {
            ammoText.color = Color.white; // Reset ammo text to white
        }
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

        if (isAiming)
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, adsFOV, smoothSpeed);

            // Move the specified adsObject during ADS (instead of using the transform of this script)
            if (adsObject != null)
            {
                adsObject.transform.localPosition = Vector3.Lerp(adsObject.transform.localPosition, adsPosition, smoothSpeed);
            }
            //Debug.Log("ADS Position: " + adsObject.transform.localPosition);
        }
        else
        {
            camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, normalFOV, smoothSpeed);

            // Return the adsObject to the normal position
            if (adsObject != null)
            {
                adsObject.transform.localPosition = Vector3.Lerp(adsObject.transform.localPosition, normalPosition, smoothSpeed);
            }
            //Debug.Log("Normal Position: " + adsObject.transform.localPosition);
        }
    }


    public void HandleSingleShotFire()
    {
        if (Input.GetButtonDown("Fire1") && nextFire <= 0 && ammo > 0 && !isReloading)
        {
            nextFire = fireRate; // Fire every fireRate seconds

            ammo--;
            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;
            animator.SetTrigger(recoilTrigger);

            Fire();

            // Set isFiring to true and reset after fireRate seconds
            StartCoroutine(ResetFiringAfterDelay(fireRate));
        }
    }

    public void HandleAutomaticFire()
    {
        if (Input.GetButton("Fire1") && nextFire <= 0 && ammo > 0 && !isReloading)
        {
            nextFire = fireRate; // Fire every fireRate seconds

            ammo--;
            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;
            animator.SetTrigger(recoilTrigger);

            Fire();
        }

        // Stop firing if the button is released
        if (Input.GetButtonUp("Fire1"))
        {
            StopFiring();
        }
    }

    public void HandleBurstFire()
    {
        if (Input.GetButtonDown("Fire1") && burstCoroutine == null && ammo > 0 && !isReloading)
        {
            burstCoroutine = StartCoroutine(FireBurst());
        }
    }

    IEnumerator FireBurst()
    {
        isFiring = true; // Set isFiring to true during the burst

        int shotsFired = 0;

        while (shotsFired < burstCount && ammo > 0)
        {
            nextFire = burstFireRate; // Delay between burst shots
            ammo--;
            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;

            Fire();
            shotsFired++;

            yield return new WaitForSeconds(burstFireRate);
        }

        // Reset isFiring and allow another burst immediately
        isFiring = false;
        nextFire = fireRate; // After the burst ends, apply standard fire rate cooldown
        burstCoroutine = null;
    }

    public void Reload()
    {
        if (mag > 0)
        {
            isReloading = true;
            animator.SetTrigger(reloadTrigger); 
            ikHandler.StartReloadIK();  // Start IK handling when reloading begins
            StartCoroutine(ReloadRoutine(animator.GetCurrentAnimatorStateInfo(0).length));
        }
    }

    IEnumerator ReloadRoutine(float reloadTime)
    {
        yield return new WaitForSeconds(0.2f); // Wait a bit before the hand starts moving

        magPosition.gameObject.SetActive(false); // Simulate mag removal

        yield return new WaitForSeconds(reloadTime / 2); // Wait to simulate replacement

        magPosition.gameObject.SetActive(true); // Simulate mag insertion

        yield return new WaitForSeconds(reloadTime / 2);

        mag--;
        ammo = magAmmo;

        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;

        isReloading = false;
        ikHandler.StopReloadIK();  // Stop IK handling when reload is done
    }

    public void Fire()
    {
        isFiring = true;

        // Adjust the animator's speed based on fireRate
        animator.speed = 1 / fireRate;

        // Muzzle flash effect
        GameObject flashInstance = Instantiate(muzzleFlash, muzzleTransform.position, muzzleTransform.rotation);
        Destroy(flashInstance, muzzleFlashDuration);

        // Calculate recoil with randomness
        Vector2 recoilOffset = Vector2.zero;
        if (currentRecoilIndex < recoilPattern.Length)
        {
            recoilOffset = recoilPattern[currentRecoilIndex] + new Vector2(Random.Range(-recoilRandomFactor, recoilRandomFactor), Random.Range(-recoilRandomFactor, recoilRandomFactor));
            currentRecoilIndex++;
        }

        // Apply spread
        Vector3 spreadOffset = new Vector3(
            Random.Range(-currentSpread, currentSpread),
            Random.Range(-currentSpread, currentSpread),
            0
        );

        // Adjust fire direction with recoil and spread
        Vector3 fireDirection = camera.transform.forward + camera.transform.right * (recoilOffset.x + spreadOffset.x) + camera.transform.up * (recoilOffset.y + spreadOffset.y);
        camera.transform.localRotation *= Quaternion.Euler(-recoilOffset.y, recoilOffset.x, 0);

        Ray ray = new Ray(camera.transform.position, fireDirection);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);

            Health targetHealth = hit.transform.gameObject.GetComponent<Health>();
            if (targetHealth != null) // Ensure the hit object has a Health component
            {
                int remainingHealth = targetHealth.health - damage;

                hit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);

                if (remainingHealth <= 0)
                {
                    RoomManager.instance.kills++;
                    RoomManager.instance.SetHashes();
                }
            }
        }

        // Increase spread
        currentSpread = Mathf.Min(currentSpread + spreadIncreasePerShot, maxSpread);

        // Reset recoil timer
        recoilTimer = recoilResetTime;
    }

    public void StopFiring()
    {
        isFiring = false; // Stop firing, set the bool to false

        // Reset the animator's speed to normal
        animator.speed = 1;
    }

    // Method to reset isFiring after a delay (used for single-shot and burst fire)
    IEnumerator ResetFiringAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isFiring = false;
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

    public void UpdateWeaponUI()
    {
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;
        fireModeText.text = currentFireMode.ToString();
    }
}
