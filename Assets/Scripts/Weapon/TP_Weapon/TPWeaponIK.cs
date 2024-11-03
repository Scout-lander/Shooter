using UnityEngine;

public class TPWeaponIK : MonoBehaviour
{
    [Header("IK Targets for TP Model")]
    public Transform rightHandIKTarget; // Assign in the inspector
    public Transform leftHandIKTarget;  // Assign in the inspector

    // You could also include other weapon-specific settings such as animations, effects, etc.
}
