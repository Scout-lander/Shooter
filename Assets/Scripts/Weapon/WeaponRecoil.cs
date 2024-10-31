using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [SerializeField] private CameraRecoil cameraRecoil; // Handles camera recoil
    [SerializeField] private Transform gunTransform; // Transform affected by gun kick

    // Rotation recoil values for the camera
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    private Quaternion originalGunRotation;

    [Header("HipFire")]
    [SerializeField] private float hipRecoilX;
    [SerializeField] private float hipRecoilY;
    [SerializeField] private float hipRecoilZ;

    [SerializeField] private float hipGunRotationX; // X-axis rotation for hip-fire kick

    [Header("AimFire")]
    [SerializeField] private float aimRecoilX;
    [SerializeField] private float aimRecoilY;
    [SerializeField] private float aimRecoilZ;

    [SerializeField] private float aimGunRotationX; // X-axis rotation for aim-fire kick

    [Header("Settings")]
    [SerializeField] private float snappiness = 5f;
    [SerializeField] private float returnSpeed = 2f;
    [SerializeField] private Vector3 recoilClamp = new Vector3(10f, 10f, 10f); // Max recoil for the camera

    void Start()
    {
        // Store the original rotation of the gun transform
        originalGunRotation = gunTransform.localRotation;
    }

    void OnEnable()
    {
        // When this weapon is equipped, update the CameraRecoil with these values
        if (cameraRecoil != null)
        {
            cameraRecoil.SetRecoilValues(
                hipRecoilX, hipRecoilY, hipRecoilZ, 
                aimRecoilX, aimRecoilY, aimRecoilZ, 
                snappiness, returnSpeed);
        }
    }

    void Update()
    {
        // Smoothly return the gun to its original rotation after kickback
        currentRotation = Vector3.Lerp(currentRotation, Vector3.zero, returnSpeed * Time.deltaTime);

        // Apply rotations to simulate recoil
        gunTransform.localRotation = Quaternion.Euler(currentRotation) * originalGunRotation;
    }

    /// <summary>
    /// Applies kickback to the gun based on whether the player is aiming.
    /// </summary>
    public void GunKick(bool isAiming)
    {
        if (isAiming)
        {
            // Apply recoil rotation for aiming fire
            currentRotation.x -= aimGunRotationX;
        }
        else
        {
            // Apply recoil rotation for hip fire
            currentRotation.x -= hipGunRotationX;
        }

        // Clamp the rotation to prevent excessive recoil
        currentRotation = Vector3.ClampMagnitude(currentRotation, recoilClamp.magnitude);
    }
}
