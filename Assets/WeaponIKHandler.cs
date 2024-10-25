using UnityEngine;

public class WeaponIKHandler : MonoBehaviour
{
    public Animator animator;       // Animator to control IK
    public Transform leftHandIKTarget;  // Where the left hand should move during reload (near the mag)
    public Transform magTransform;  // The magazine transform, used for simulating the mag being pulled down
    public Vector3 magPulledDownPosition; // Position of the magazine when "pulled down"
    public float ikSpeed = 0.1f;    // Speed at which the hand moves to the target
    
    private float ikWeight = 0;     // IK blending value
    private bool isReloading = false;
    private Vector3 initialMagPosition; // Store the initial position of the magazine

    void Start()
    {
        // Ensure animator is assigned, can assign via Inspector or GetComponent
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Store the initial position of the magazine
        if (magTransform != null)
        {
            initialMagPosition = magTransform.localPosition;
        }
    }

    // Method to start the IK handling
    public void StartReloadIK()
    {
        isReloading = true;
    }

    // Method to stop the IK handling after reload is complete
    public void StopReloadIK()
    {
        isReloading = false;

        // Reset the magazine to its initial position after reload
        if (magTransform != null)
        {
            magTransform.localPosition = initialMagPosition;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (isReloading)
        {
            // Gradually increase IK weight to move the left hand towards the mag
            ikWeight = Mathf.Lerp(ikWeight, 1, ikSpeed * Time.deltaTime);

            // Apply IK to the left hand
            if (leftHandIKTarget != null)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
            }

            // Simulate pulling the magazine down
            if (magTransform != null)
            {
                magTransform.localPosition = Vector3.Lerp(magTransform.localPosition, magPulledDownPosition, ikSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Gradually decrease IK weight to return left hand to normal position
            ikWeight = Mathf.Lerp(ikWeight, 0, ikSpeed * Time.deltaTime);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);
        }
    }
}
