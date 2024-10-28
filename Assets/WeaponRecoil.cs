using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    
    [SerializeField] private CameraRecoil cameraRecoil;

    // Booleans

    // Rotation
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [Header("HipFire")]
    [SerializeField] private float hipRecoilX;
    [SerializeField] private float hipRecoilY;
    [SerializeField] private float hipRecoilZ;

    [Header("AimFire")]
    [SerializeField] private float aimRecoilX;
    [SerializeField] private float aimRecoilY;
    [SerializeField] private float aimRecoilZ;
    
    [Header("Settings")]
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
    [SerializeField] private Vector3 recoilClamp = new Vector3(10f, 10f, 10f); // Clamp for max recoil

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
}
