using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon
{
    // Configuration Fields
    [Header("Camera & Controller")]
    public Camera camera;
    [SerializeField] private PlayerController player;

    [Header("Recoil & Effects")]
    [SerializeField] private WeaponRecoil weaponRecoil;
    public CameraRecoil Recoil;

    [Header("Weapon Stats")]
    public int damage;
    public float fireRate = 0.3f;
    public int mag = 5;
    public int ammo = 30;
    public int magAmmo = 30;

    public enum FireMode { Single, Auto, Burst }
    public FireMode currentFireMode = FireMode.Single;
    public List<FireMode> availableFireModes = new List<FireMode> { FireMode.Single };
    public int burstCount = 3;
    public float burstFireRate = 0.1f;

    [Header("Ammo UI")]
    public TextMeshProUGUI magText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI fireModeText;

    [Header("Visual Effects")]
    public GameObject hitVFX;
    public GameObject muzzleFlash;
    public Transform muzzleTransform;
    public float muzzleFlashDuration = 0.05f;

    [Header("Animator")]
    public Animator animator;
    private string reloadTrigger = "reload";
    private string recoilTrigger = "recoil";

    [Header("Aiming Down Sights (ADS)")]
    public GameObject adsObject;
    public GameObject adsSight;
    public bool isAiming = false;
    public float adsFOV = 40f;
    public float normalFOV = 60f;
    public Vector3 normalPosition = new Vector3(0.5f, -0.5f, 1.0f);
    public Vector3 adsOffset;
    public float adsSpeed = 0.2f;

    [Header("Sound Effects")]
    public int shootSFXIndex = 0;
    public PlayerPhotonSound photonSound;

    [Header("Hand IK Targets")]
    public Transform rightHandIK;
    public Transform leftHandIK;

    // Private Variables
    private float nextFire;
    private bool isReloading = false;
    private bool isFiring = false;
    private Coroutine burstCoroutine;
    private int lowAmmoThreshold = 5;



    void Start()
    {
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;
        UpdateFireModeUI();
        UpdateWeaponUI();
    }

    void Update()
    {
        if (nextFire > 0)
        {
            nextFire -= Time.deltaTime;
        }

        isReloading = animator.GetCurrentAnimatorStateInfo(0).IsName("reload");

        if (Input.GetKeyDown(KeyCode.B))
        {
            SwitchFireMode();
        }

         if (currentFireMode == FireMode.Auto) HandleAutomaticFire();
        else if (currentFireMode == FireMode.Single) HandleSingleShotFire();
        else if (currentFireMode == FireMode.Burst) HandleBurstFire();

        if (Input.GetKeyDown(KeyCode.R) && !isReloading && mag > 0)
        {
            Reload();
        }

        HandleADS();
        HandleLowAmmoWarning();
    }

    void HandleLowAmmoWarning()
    {
        ammoText.color = ammo <= lowAmmoThreshold ? Color.red : Color.white;
    }

    void HandleADS()
    {
        float smoothSpeed = Time.deltaTime * adsSpeed;
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = true;
            player.isADS = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            isAiming = false;
            player.isADS = false;
        }

        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, isAiming ? adsFOV : normalFOV, smoothSpeed);
        Vector3 targetPosition = isAiming ? (normalPosition - adsObject.transform.InverseTransformPoint(adsSight.transform.position) + adsOffset) : normalPosition;
        if (adsObject != null) adsObject.transform.localPosition = Vector3.Lerp(adsObject.transform.localPosition, targetPosition, smoothSpeed);
    }

    public void HandleSingleShotFire()
    {
        if (Input.GetButtonDown("Fire1") && nextFire <= 0 && ammo > 0 && !isReloading)
        {
            nextFire = fireRate;
            ammo--;
            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;
            animator.SetTrigger(recoilTrigger);

            Fire();
            Recoil.RecoilFire();
            weaponRecoil.GunKick(isAiming);

            StartCoroutine(ResetFiringAfterDelay(fireRate));
        }
    }

    public void HandleAutomaticFire()
    {
        if (Input.GetButton("Fire1") && nextFire <= 0 && ammo > 0 && !isReloading)
        {
            nextFire = fireRate;
            ammo--;
            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;
            animator.SetTrigger(recoilTrigger);

            Fire();
            Recoil.RecoilFire();
            weaponRecoil.GunKick(isAiming);
        }

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
        isFiring = true;
        int shotsFired = 0;

        while (shotsFired < burstCount && ammo > 0)
        {
            nextFire = burstFireRate;
            ammo--;
            magText.text = mag.ToString();
            ammoText.text = ammo + "/" + magAmmo;

            Fire();
            Recoil.RecoilFire();
            weaponRecoil.GunKick(isAiming);
            shotsFired++;

            yield return new WaitForSeconds(burstFireRate);
        }

        isFiring = false;
        nextFire = fireRate;
        burstCoroutine = null;
    }

    public void Reload()
    {
        if (mag > 0)
        {
            isReloading = true;
            animator.SetTrigger(reloadTrigger); 
            StartCoroutine(ReloadRoutine(animator.GetCurrentAnimatorStateInfo(0).length));
        }
    }

    IEnumerator ReloadRoutine(float reloadTime)
    {
        yield return new WaitForSeconds(0.2f);

        mag--;
        ammo = magAmmo;
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;

        isReloading = false;
    }

    public void Fire()
    {
        isFiring = true;
        animator.speed = 1 / fireRate;

        photonSound.PlayShootSFX(shootSFXIndex);

        GameObject flashInstance = Instantiate(muzzleFlash, muzzleTransform.position, muzzleTransform.rotation);
        Destroy(flashInstance, muzzleFlashDuration);

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            PhotonNetwork.Instantiate(hitVFX.name, hit.point, Quaternion.identity);

            Health targetHealth = hit.transform.gameObject.GetComponent<Health>();
            PhotonView targetPhotonView = hit.transform.gameObject.GetComponent<PhotonView>();

            // Only call TakeDamage on other clients if the target has a PhotonView
            if (targetPhotonView != null)
            {
                targetPhotonView.RPC("TakeDamage", RpcTarget.OthersBuffered, damage, PhotonNetwork.LocalPlayer.NickName);
            }
        }
    }

    public void StopFiring()
    {
        isFiring = false;
        animator.speed = 1;
    }
    

    IEnumerator ResetFiringAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isFiring = false;
    }

    public void SwitchFireMode()
    {
        int currentIndex = availableFireModes.IndexOf(currentFireMode);
        currentFireMode = availableFireModes[(currentIndex + 1) % availableFireModes.Count];
        UpdateFireModeUI();
    }

     public void UpdateFireModeUI()
    {
        fireModeText.text = currentFireMode.ToString();
    }

     public void UpdateWeaponUI()
    {
        magText.text = mag.ToString();
        ammoText.text = ammo + "/" + magAmmo;
        UpdateFireModeUI();
    }
}
