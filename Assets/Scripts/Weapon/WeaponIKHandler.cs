using UnityEngine;

public class WeaponIKHandler : MonoBehaviour
{
    public Animator animator;

    [Header("IK Targets")]
    public Transform rightHandTarget; // Target position for the right hand on the weapon
    public Transform leftHandTarget;  // Target position for the left hand on the weapon

    [Header("Hand Bones")]
    public Transform rightHandBone;   // Character’s right hand bone
    public Transform leftHandBone;    // Character’s left hand bone

    [Range(0, 1)] public float ikWeight = 1.0f; // Adjust IK weight to blend smoothly

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            // Blend between bone position and IK position for the right hand
            if (rightHandTarget != null && rightHandBone != null)
            {
                // Set the base position and rotation to the hand bone
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, ikWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, ikWeight);

                // Blend the position and rotation between the bone and IK target
                Vector3 blendedRightPosition = Vector3.Lerp(rightHandBone.position, rightHandTarget.position, ikWeight);
                Quaternion blendedRightRotation = Quaternion.Slerp(rightHandBone.rotation, rightHandTarget.rotation, ikWeight);

                animator.SetIKPosition(AvatarIKGoal.RightHand, blendedRightPosition);
                animator.SetIKRotation(AvatarIKGoal.RightHand, blendedRightRotation);
            }

            // Blend between bone position and IK position for the left hand
            if (leftHandTarget != null && leftHandBone != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, ikWeight);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, ikWeight);

                Vector3 blendedLeftPosition = Vector3.Lerp(leftHandBone.position, leftHandTarget.position, ikWeight);
                Quaternion blendedLeftRotation = Quaternion.Slerp(leftHandBone.rotation, leftHandTarget.rotation, ikWeight);

                animator.SetIKPosition(AvatarIKGoal.LeftHand, blendedLeftPosition);
                animator.SetIKRotation(AvatarIKGoal.LeftHand, blendedLeftRotation);
            }
        }
    }
}
