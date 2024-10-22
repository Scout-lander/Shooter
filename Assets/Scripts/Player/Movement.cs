using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class Movement : MonoBehaviourPun
{
    private float walkSpeed;
    private float sprintSpeed;
    private float crouchSpeed;
    private float slideSpeed;
    public float slideDuration = 1f;
    public float accelerationTime = 0.5f;
    public float decelerationTime = 0.3f;


    [Header("Air Control")]
    public float airControlFactor = 0.2f; // Partial control in the air
    public float airDrag = 0.95f; // How much air movement slows down

    [Header("Jump")]
    public float jumpHeight = 5f;

    private Vector2 input;
    public CharacterController controller;

    [HideInInspector] public bool sprinting;
    [HideInInspector] public bool jumping;
    [HideInInspector] public bool crouching;
    [HideInInspector] public bool sliding;
    public bool grounded = false;

    private float currentSpeed = 0f; // Smooth speed transition variable
    private float targetSpeed = 0f;  // The speed we are targeting

    private Vector3 velocitySmoothing; // Used to smooth velocity changes
    private Vector3 moveDirection;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float staminaUseRate = 20f;
    public float staminaRegenRate = 10f;
    private float currentStamina;

    [Header("Head Bobbing")]
    public Transform playerCamera;
    public float bobbingSpeed = 0.18f;
    public float bobbingAmount = 0.05f;
    private float defaultPosY = 0;
    private float bobTimer = 0;

    private float slideTimer;  // Added slideTimer

    void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        controller = GetComponent<CharacterController>();
        currentStamina = maxStamina;
        defaultPosY = playerCamera.localPosition.y; // Initialize the default Y position for head bobbing
    }

    public void SetMovementStats(PlayerClass playerClass)
    {
        walkSpeed = playerClass.walkSpeed;
        sprintSpeed = playerClass.sprintSpeed;
        crouchSpeed = playerClass.crouchSpeed;
        slideSpeed = playerClass.slideSpeed;
        jumpHeight = playerClass.jumpHeight;
        staminaUseRate = playerClass.staminaUseRate;
        staminaRegenRate = playerClass.staminaRegenRate;
    }

    void Update()
    {
        GroundCheck();
        ManageStamina();
        HandleHeadBobbing();

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Normalize();

        // Set target speed based on sprinting, walking, crouching, or sliding
        if (sliding)
        {
            targetSpeed = slideSpeed;
        }
        else if (crouching && grounded && !sprinting)
        {
            targetSpeed = crouchSpeed;
        }
        else if (input.magnitude > 0.5f)
        {
            targetSpeed = sprinting ? sprintSpeed : walkSpeed;
        }
        else
        {
            targetSpeed = 0f; // If no input, slow down to a stop
        }

        // Update sliding timer
        if (sliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                EndSlide();
            }
        }
    }

    // This method will be called by PlayerController to handle actual movement
    public void MovePlayer()
    {
        if (grounded)
        {
            if (jumping && !sliding)
            {
                moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            }

            // Smoothly adjust the player's speed based on target speed (walking/sprinting/crouching/sliding)
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref velocitySmoothing.x, input.magnitude > 0.5f ? accelerationTime : decelerationTime);

            Vector3 move = transform.right * input.x + transform.forward * input.y;
            moveDirection.x = move.x * currentSpeed;
            moveDirection.z = move.z * currentSpeed;
        }
        else
        {
            // Apply air drag and allow partial control while not grounded
            moveDirection.x *= airDrag;
            moveDirection.z *= airDrag;
        }

        // Apply gravity
        moveDirection.y += Physics.gravity.y * Time.deltaTime;

        // Move character controller
        controller.Move(moveDirection * Time.deltaTime);
    }

    // Start sliding
    public void StartSlide()
    {
        sliding = true;
        slideTimer = slideDuration;  // Reset the slide timer when sliding starts
        moveDirection = transform.forward * slideSpeed;
    }

    // End sliding
    public void EndSlide()
    {
        sliding = false;
    }

    // Check if the player is grounded
    private void GroundCheck()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (grounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f; // Small downward force to keep grounded
        }
    }

    // Manage stamina based on sprinting and regeneration
    void ManageStamina()
    {
        if (sprinting && currentStamina > 0)
        {
            currentStamina -= staminaUseRate * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    // Handle head bobbing effect during movement
    void HandleHeadBobbing()
    {
        if (input.magnitude > 0.5f && !sliding)
        {
            bobTimer += Time.deltaTime * (sprinting ? sprintSpeed : walkSpeed) / walkSpeed * bobbingSpeed;
            playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, defaultPosY + Mathf.Sin(bobTimer) * bobbingAmount, playerCamera.localPosition.z);
        }
        else
        {
            bobTimer = 0;
            playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, Mathf.Lerp(playerCamera.localPosition.y, defaultPosY, Time.deltaTime * 5), playerCamera.localPosition.z);
        }
    }
}