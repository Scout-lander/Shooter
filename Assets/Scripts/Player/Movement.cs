using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Movement : MonoBehaviourPun
{
    private float walkSpeed;
    private float sprintSpeed;
    private float crouchSpeed;
    private float moveSpeed; // Current movement speed

    public float accelerationTime = 0.5f;
    public float decelerationTime = 0.3f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1.0f;
    private float standHeight;
    public float crouchTransitionSpeed = 5f;

    [Header("Air Control")]
    public float airControlFactor = 0.2f;
    public float airDrag = 0.95f;

    [Header("Jump")]
    public float jumpHeight = 5f;

    [Header("Animation")]
    public Animator animator; // Use Animator instead of Animation
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private Vector2 input;
    private Vector3 velocitySmoothing;
    private Vector3 moveDirection;

    public CharacterController controller;

    [HideInInspector] public bool sprinting;
    [HideInInspector] public bool jumping;
    [HideInInspector] public bool crouching;
    public bool grounded = false;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 90f;
    private float xRotation = 0f;

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
    private float sprintBobbingSpeed = 0.25f;
    private float sprintBobbingAmount = 0.1f;
    private float defaultPosY = 0;
    private float bobTimer = 0;

    void Start()
    {
        if (!photonView.IsMine) return;

        controller = GetComponent<CharacterController>();
        standHeight = controller.height;
        currentStamina = maxStamina;
        defaultPosY = playerCamera.localPosition.y;

        InitializeStaminaUI();
    }

    private void InitializeStaminaUI()
    {
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
        if (!photonView.IsMine) return;

        GroundCheck();
        ManageStamina();
        HandleHeadBobbing();
        HandleMouseLook();

        UpdateInput();
        UpdateMovement();
        UpdateAnimation();
    }

    private void UpdateInput()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    private void UpdateMovement()
    {
        moveSpeed = DetermineMoveSpeed();

        if (grounded)
        {
            HandleGroundMovement();
            AdjustCharacterHeight();
        }
        else
        {
            HandleAirMovement();
        }

        controller.Move(moveDirection * Time.deltaTime);
    }

    private float DetermineMoveSpeed()
    {
        if (crouching && grounded && !sprinting) return crouchSpeed;
        return input.magnitude > 0.5f ? (sprinting ? sprintSpeed : walkSpeed) : 0f;
    }

    private void HandleGroundMovement()
    {
        if (jumping)
            moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);

        float targetSpeed = moveSpeed;
        moveSpeed = Mathf.SmoothDamp(moveSpeed, targetSpeed, ref velocitySmoothing.x, input.magnitude > 0.5f ? accelerationTime : decelerationTime);

        Vector3 move = transform.right * input.x + transform.forward * input.y;
        moveDirection.x = move.x * moveSpeed;
        moveDirection.z = move.z * moveSpeed;
    }

    private void HandleAirMovement()
    {
        moveDirection.x *= airDrag;
        moveDirection.z *= airDrag;
        moveDirection.y += Physics.gravity.y * Time.deltaTime;
    }

    private void AdjustCharacterHeight()
    {
        controller.height = Mathf.Lerp(controller.height, crouching ? crouchHeight : standHeight, Time.deltaTime * crouchTransitionSpeed);
    }

    private void UpdateAnimation()
    {
        animator.SetFloat(SpeedHash, moveSpeed); // Update animator speed
        animator.SetBool("isSprinting", sprinting);
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void GroundCheck()
    {
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (grounded && moveDirection.y < 0)
        {
            moveDirection.y = -2f;
        }
    }

    private void ManageStamina()
    {
        if (sprinting && currentStamina > 0)
        {
            currentStamina -= staminaUseRate * Time.deltaTime;
            if (currentStamina <= 0) sprinting = false;
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        if (staminaSlider != null)
            staminaSlider.value = currentStamina;
    }

    private void HandleHeadBobbing()
    {
        float currentBobbingSpeed = sprinting ? sprintBobbingSpeed : bobbingSpeed;
        float currentBobbingAmount = sprintBobbingAmount;

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
