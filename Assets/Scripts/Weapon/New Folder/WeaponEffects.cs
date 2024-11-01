using UnityEngine;

public class WeaponEffects : MonoBehaviour
{
    public GameObject muzzleFlashPrefab; // Assign this in the Inspector
    public Transform muzzlePosition;     // Assign this in the Inspector

    // Method to create the muzzle flash effect
    public void CreateMuzzleFlash()
    {
        if (muzzleFlashPrefab && muzzlePosition)
        {
            GameObject flashInstance = Instantiate(muzzleFlashPrefab, muzzlePosition.position, Quaternion.identity, muzzlePosition);
            Destroy(flashInstance, 0.05f); // Adjust time as necessary
        }
    }

    // Other existing code...
}
