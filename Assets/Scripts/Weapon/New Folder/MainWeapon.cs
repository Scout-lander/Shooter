using UnityEngine;

public class MainWeapon : MonoBehaviour
{
    // Reference to the specialized weapon components
    [Header("Weapon Components")]
    public WeaponFireControl fireControl;
    public WeaponAmmo ammoControl;
    public Recoil recoilControl;
    public WeaponADS adsControl;
    public WeaponEffects effectsControl;

    [Header("Weapon Properties")]
    public Camera playerCamera;
    public Transform weaponTransform; // The main transform of the weapon where effects might be parented

    void Awake()
    {
        // Ensure all weapon components are set
        if (fireControl == null) fireControl = GetComponent<WeaponFireControl>();
        if (ammoControl == null) ammoControl = GetComponent<WeaponAmmo>();
        if (recoilControl == null) recoilControl = GetComponent<Recoil>();
        if (adsControl == null) adsControl = GetComponent<WeaponADS>();
        if (effectsControl == null) effectsControl = GetComponent<WeaponEffects>();
    }

    void Update()
    {
        // Handle weapon input if necessary
        HandleInput();
    }

    private void HandleInput()
    {
        // Input handling that affects weapon behavior can be centralized here
        if (Input.GetKeyDown(KeyCode.R))
        {
            ammoControl.Reload();
        }

        if (Input.GetMouseButtonDown(1)) // Right Mouse Button for ADS
        {
            adsControl.ToggleADS(true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            adsControl.ToggleADS(false);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }

        // You might want fire control handling directly in the update loop of the FireControl script
        // However, you can also trigger them here based on specific game conditions or input
    }

    public void Fire()
    {
        if (ammoControl.CanFire())
        {
            effectsControl.CreateMuzzleFlash();
            fireControl.Fire();
        }
    }
}
