using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles all the Inverse Kinematics needed for the character's arms to align with weapon targets, including hints for elbow positioning.
/// </summary>
public class CharacterKinematics : MonoBehaviour
{
    [Header("Settings Arm Left")]
    [Tooltip("Left Arm Target. Determines what the IK target is.")]
    [SerializeField] private Transform armLeftTarget;

    [Range(0.0f, 1.0f)]
    [Tooltip("Inverse Kinematics Weight for the left arm.")]
    [SerializeField] private float armLeftWeightPosition = 1.0f;

    [Range(0.0f, 1.0f)]
    [Tooltip("Inverse Kinematics Weight for the left arm.")]
    [SerializeField] private float armLeftWeightRotation = 1.0f;

    [Tooltip("Left Arm Hierarchy. Root, Mid, Tip.")]
    [SerializeField] private Transform[] armLeftHierarchy;

    [Header("Settings Arm Right")]
    [Tooltip("Right Arm Target. Determines what the IK target is.")]
    [SerializeField] private Transform armRightTarget;

    [Range(0.0f, 1.0f)]
    [Tooltip("Inverse Kinematics Weight for the right arm.")]
    [SerializeField] private float armRightWeightPosition = 1.0f;

    [Range(0.0f, 1.0f)]
    [Tooltip("Inverse Kinematics Weight for the right arm.")]
    [SerializeField] private float armRightWeightRotation = 1.0f;

    [Tooltip("Right Arm Hierarchy. Root, Mid, Tip.")]
    [SerializeField] private Transform[] armRightHierarchy;

    [Header("Generic")]
    [Tooltip("Hint for elbow positioning.")]
    [SerializeField] private Transform hint;

    [Range(0.0f, 1.0f)]
    [Tooltip("Hint Weight.")]
    [SerializeField] private float weightHint = 1.0f;

    private const float KSqrEpsilon = 1e-8f;

    void LateUpdate()
    {
        // Compute IK for both arms
        Compute(1.0f, 1.0f);
    }

    /// <summary>
    /// Computes the Inverse Kinematics for both arms.
    /// </summary>
    public void Compute(float weightLeft = 1.0f, float weightRight = 1.0f)
    {
        // Compute Left Arm IK
        ComputeOnce(armLeftHierarchy, armLeftTarget,
            armLeftWeightPosition * weightLeft,
            armLeftWeightRotation * weightLeft);

        // Compute Right Arm IK
        ComputeOnce(armRightHierarchy, armRightTarget,
            armRightWeightPosition * weightRight,
            armRightWeightRotation * weightRight);
    }

    /// <summary>
    /// Computes the Inverse Kinematics for a single arm hierarchy.
    /// </summary>
    private void ComputeOnce(IReadOnlyList<Transform> hierarchy, Transform target, float weightPosition, float weightRotation)
    {
        Vector3 targetPosition = Vector3.Lerp(hierarchy[2].position, target.position, weightPosition);
        Quaternion targetRotation = Quaternion.Slerp(hierarchy[2].rotation, target.rotation, weightRotation);

        // Position and rotation of each bone in the hierarchy
        Vector3 aPosition = hierarchy[0].position; // Root
        Vector3 bPosition = hierarchy[1].position; // Mid
        Vector3 cPosition = hierarchy[2].position; // Tip

        Vector3 ab = bPosition - aPosition;
        Vector3 bc = cPosition - bPosition;
        Vector3 at = targetPosition - aPosition;

        float abLen = ab.magnitude;
        float bcLen = bc.magnitude;
        float atLen = at.magnitude;

        // Calculate the angle for the elbow based on distances
        float oldAbcAngle = TriangleAngle((cPosition - aPosition).magnitude, abLen, bcLen);
        float newAbcAngle = TriangleAngle(atLen, abLen, bcLen);

        // Compute bend normal using either animation or hint direction
        Vector3 bendNormal = Vector3.Cross(ab, bc);
        if (bendNormal.sqrMagnitude < KSqrEpsilon)
        {
            bendNormal = hint != null ? Vector3.Cross(hint.position - aPosition, bc) : Vector3.Cross(at, bc);
            if (bendNormal.sqrMagnitude < KSqrEpsilon)
                bendNormal = Vector3.up; // Fallback direction
        }
        bendNormal = bendNormal.normalized;

        // Rotate elbow angle to match target position
        float rotationAngle = 0.5f * (oldAbcAngle - newAbcAngle);
        Quaternion elbowRotation = Quaternion.AngleAxis(rotationAngle * Mathf.Rad2Deg, bendNormal);
        hierarchy[1].rotation = elbowRotation * hierarchy[1].rotation;

        // Adjust shoulder rotation to align arm direction to target
        hierarchy[0].rotation = Quaternion.FromToRotation(cPosition - aPosition, at) * hierarchy[0].rotation;

        // Apply hint weight if hint is available
        if (hint != null && weightHint > 0f)
        {
            Vector3 hintDir = hint.position - aPosition;
            Vector3 projectedHintDir = hintDir - Vector3.Project(hintDir, at);
            Vector3 elbowDir = bPosition - aPosition;

            Quaternion hintRotation = Quaternion.FromToRotation(elbowDir, projectedHintDir);
            hierarchy[0].rotation = Quaternion.Slerp(Quaternion.identity, hintRotation, weightHint) * hierarchy[0].rotation;
        }

        // Final hand alignment
        hierarchy[2].position = targetPosition;
        hierarchy[2].rotation = targetRotation;
    }

    /// <summary>
    /// Calculates the angle of a triangle side using the law of cosines.
    /// </summary>
    private static float TriangleAngle(float aLen, float bLen, float cLen)
    {
        float cosAngle = Mathf.Clamp((bLen * bLen + cLen * cLen - aLen * aLen) / (2.0f * bLen * cLen), -1.0f, 1.0f);
        return Mathf.Acos(cosAngle);
    }
}
