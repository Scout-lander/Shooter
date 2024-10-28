using Unity.VisualScripting;
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    // Scripts
    [SerializeField] private PlayerController player;

    // Booleans
    private bool isAiming;

    // Rotation
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [Header("HipFire")]
    private float recoilX;
    private float recoilY;
    private float recoilZ;

    [Header("AimFire")]
    private float aimRecoilX;
    private float aimRecoilY;
    private float aimRecoilZ;

    [Header("Settings")]
    private float snappiness;
    private float returnSpeed;
    [SerializeField] private Vector3 recoilClamp = new Vector3(10f, 10f, 10f); // Clamp for max recoil

    void Start()
    {
        currentRotation = transform.localRotation.eulerAngles;
        targetRotation = Vector3.zero;
    }

    void Update()
    {
        isAiming = player.isADS;

        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);
        
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void SetRecoilValues(float hipX, float hipY, float hipZ, float aimX, float aimY, float aimZ, float snap, float returnSpd)
    {
        recoilX = hipX;
        recoilY = hipY;
        recoilZ = hipZ;
        aimRecoilX = aimX;
        aimRecoilY = aimY;
        aimRecoilZ = aimZ;
        snappiness = snap;
        returnSpeed = returnSpd;
    }

    public void RecoilFire()
    {
        if (isAiming)
        {
            targetRotation += new Vector3(aimRecoilX, Random.Range(-aimRecoilY, aimRecoilY), Random.Range(-aimRecoilZ, aimRecoilZ));
        }
        else
        {
            targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), Random.Range(-recoilZ, recoilZ));
        }

        // Clamp target rotation to prevent excessive recoil accumulation
        targetRotation.x = Mathf.Clamp(targetRotation.x, -recoilClamp.x, recoilClamp.x);
        targetRotation.y = Mathf.Clamp(targetRotation.y, -recoilClamp.y, recoilClamp.y);
        targetRotation.z = Mathf.Clamp(targetRotation.z, -recoilClamp.z, recoilClamp.z);
    }
}
