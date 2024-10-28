using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sway : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Weapon weapon;
    private bool isAiming;

    [Header("HipFire")]
    public float hipSwayClamp = 0.09f;
    public float hipSmoothing = 3f;

    [Header("AimFire")]
    public float aimSwayClamp = 0.05f;
    public float aimSmoothing = 2f;

    [Header("Transition")]
    public float transitionSpeed = 5f;

    [Header("Rotational Sway")]
    public float rotationAmount = 4f;
    public float rotationSmoothness = 2f;

    [Header("Return to Origin")]
    public float originReturnSpeed = 2f;

    private Vector3 origin;
    private Vector3 currentPosition;
    private Quaternion originRotation;
    private float currentClamp;
    private float currentSmoothing;

    void Start()
    {
        origin = transform.localPosition;
        originRotation = transform.localRotation;
        currentClamp = hipSwayClamp;
        currentSmoothing = hipSmoothing;
    }

    void Update()
    {
        isAiming = weapon.isAiming;

        // Smoothly transition between hip and aim settings
        currentClamp = Mathf.Lerp(currentClamp, isAiming ? aimSwayClamp : hipSwayClamp, Time.deltaTime * transitionSpeed);
        currentSmoothing = Mathf.Lerp(currentSmoothing, isAiming ? aimSmoothing : hipSmoothing, Time.deltaTime * transitionSpeed);

        // Get mouse input
        Vector2 input = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // Clamp input based on current sway setting
        input.x = Mathf.Clamp(input.x, -currentClamp, currentClamp);
        input.y = Mathf.Clamp(input.y, -currentClamp, currentClamp);

        // Calculate target position for positional sway
        Vector3 targetPosition = new Vector3(-input.x, -input.y, 0);
        currentPosition = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * currentSmoothing);

        // Apply positional sway with return to origin
        transform.localPosition = Vector3.Lerp(transform.localPosition, origin + currentPosition, Time.deltaTime * originReturnSpeed);

        // Calculate rotational sway based on input
        Quaternion targetRotation = Quaternion.Euler(input.y * rotationAmount, input.x * rotationAmount, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, originRotation * targetRotation, Time.deltaTime * rotationSmoothness);
    }
}
