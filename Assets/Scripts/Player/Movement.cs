using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Movement : MonoBehaviourPun
{
    private float walkSpeed;
    private float sprintSpeed;
    private float crouchSpeed;
    public float accelerationTime = 0.5f;
    public float decelerationTime = 0.3f;

    [Header("Air Control")]
    public float airControlFactor = 0.2f;
    public float airDrag = 0.95f;

    [Header("Jump")]
    public float jumpHeight = 5f;

    private Vector2 input;
    public CharacterController controller;

    [HideInInspector] public bool sprinting;
    [HideInInspector] public bool jumping;
    [HideInInspector] public bool crouching;
    public bool grounded = false;

    private float currentSpeed = 0f;
    private float targetSpeed = 0f;

    private Vector3 velocitySmoothing;
    private Vector3 moveDirection;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float staminaUseRate = 20f;
    public float staminaRegenRate = 10f;
    public float currentStamina;

    public Slider staminaSlider;

    [Header("Head Bobbing")]
    public Transform playerCamera;
    public float bobbingSpeed = 0.18f;
    public float bobbingAmount = 0.05f;
    private float sprintBobbingSpeed = 0.25f; // Increased speed while sprinting
    private float sprintBobbingAmount = 0.1f; // Increased bobbing amount while sprinting
    private float defaultPosY = 0;
    private float bobTimer = 0;

    void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        controller = GetComponent<CharacterController>();
        currentStamina = maxStamina;
        defaultPosY = playerCamera.localPosition.y;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    public void SetMovementStats(PlayerClass playerClass)
    {
        walkSpeed = playerClass.walkSpeed;
        sprintSpeed = playerClass.sprintSpeed;
        crouchSpeed = playerClass.crouchSpeed;
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

        if (crouching && grounded && !sprinting)
        {
            targetSpeed = crouchSpeed;
        }
        else if (input.magnitude > 0.5f)
        {
            targetSpeed = sprinting ? sprintSpeed : walkSpeed;
        }
        else
        {
            targetSpeed = 0f;
        }
    }

    public void MovePlayer()
    {
        if (grounded)
        {
            if (jumping)
            {
                moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            }

            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref velocitySmoothing.x, input.magnitude > 0.5f ? accelerationTime : decelerationTime);

            Vector3 move = transform.right * input.x + transform.forward * input.y;
            moveDirection.x = move.x * currentSpeed;
            moveDirection.z = move.z * currentSpeed;
        }
        else
        {
            moveDirection.x *= airDrag;
            moveDirection.z *= airDrag;
        }

        moveDirection.y += Physics.gravity.y * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);
    }

    private void GroundCheck()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (grounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f;
        }
    }

    void ManageStamina()
    {
        if (sprinting && currentStamina > 0)
        {
            currentStamina -= staminaUseRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                sprinting = false;
            }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
    }

    void HandleHeadBobbing()
    {
        float currentBobbingSpeed = sprinting ? sprintBobbingSpeed : bobbingSpeed;
        float currentBobbingAmount = sprinting ? sprintBobbingAmount : bobbingAmount;

        if (input.magnitude > 0.5f)
        {
            bobTimer += Time.deltaTime * (sprinting ? sprintSpeed : walkSpeed) / walkSpeed * currentBobbingSpeed;
            playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, defaultPosY + Mathf.Sin(bobTimer) * currentBobbingAmount, playerCamera.localPosition.z);
        }
        else
        {
            bobTimer = 0;
            playerCamera.localPosition = new Vector3(playerCamera.localPosition.x, Mathf.Lerp(playerCamera.localPosition.y, defaultPosY, Time.deltaTime * 5), playerCamera.localPosition.z);
        }
    }
}
