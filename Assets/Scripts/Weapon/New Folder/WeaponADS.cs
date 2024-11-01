using UnityEngine;

public class WeaponADS : MonoBehaviour
{
    public Camera camera;
    public float adsFOV = 40f;
    public float normalFOV = 60f;
    public Transform adsObject;
    public Vector3 normalPosition;
    public Vector3 adsPositionOffset; // Ensure this is correctly set in the Inspector or initialized elsewhere
    public float adsSpeed = 0.2f;
    private bool isAiming = false;

    void Update()
    {
        HandleADS();
    }

    public void ToggleADS(bool aiming)
    {
        isAiming = aiming;
    }

    private void HandleADS()
    {
        float targetFOV = isAiming ? adsFOV : normalFOV;
        float currentFOV = camera.fieldOfView;
        camera.fieldOfView = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * adsSpeed);

        if (adsObject != null)
        {
            Vector3 targetPosition = isAiming ? normalPosition + adsPositionOffset : normalPosition;
            adsObject.transform.localPosition = Vector3.Lerp(adsObject.transform.localPosition, targetPosition, Time.deltaTime * adsSpeed);
        }
    }
}
