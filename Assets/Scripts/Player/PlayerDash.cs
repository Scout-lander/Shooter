using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public float dashCooldown = 2f;      // Time between dashes
    public float dashDistance = 5f;      // Distance covered during a dash
    public float dashSpeed = 20f;        // Speed of the dash

    private float lastDashTime;          // Time when the last dash occurred
    private Vector3 dashDirection;       // Direction of the dash
    private bool isDashing;

    private CharacterController characterController; // Reference to CharacterController

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        lastDashTime = -dashCooldown; // Ensures dash is available at start
    }

    void Update()
    {
        // Trigger dash with middle mouse button (button index 2) if cooldown has passed
        if (Input.GetMouseButtonDown(2) && Time.time >= lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        dashDirection = transform.forward; // Dash in the direction the player is facing
        float startTime = Time.time;

        // Dash loop to cover the dash distance at dash speed
        while (Time.time < startTime + (dashDistance / dashSpeed))
        {
            characterController.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
    }
    public bool CanDash()
    {
        return Time.time >= lastDashTime + dashCooldown;
    }

    public void PerformDash()
    {
        StartCoroutine(Dash());
    }
}
