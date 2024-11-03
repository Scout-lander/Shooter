using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Movement : MonoBehaviourPun
{
    private float walkSpeed;
    private float sprintSpeed;
    private float crouchSpeed;
    private float moveSpeed;

    public float ADSSpeed = 2f; // Speed while aiming down sights
    private bool isAiming; // Track if player is in ADS mode

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
    [Header("Third-Person Weapon")]
    public Transform tpWeapon; // Assign this in the inspector
    
    [Header("Animation")]
    public Animator fpAnimator; // First-Person Animator
    public PhotonAnimatorView photonAnimatorView;
    public Animator tpAnimator; // Third-Person Animator
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private Vector2 input;
    private Vector3 velocitySmoothing;
    private Vector3 moveDirection;
    private Vector3 lastPosition; // To calculate velocity
    private Vector3 originalCameraPosition;

    public CharacterController controller;

    [HideInInspector] public bool sprinting;
    [HideInInspector] public bool jumping;
    [HideInInspector] public bool crouching;
    [HideInInspector] public bool isMoving;
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
        lastPosition = transform.position;
        originalCameraPosition = playerCamera.localPosition;

        if (photonAnimatorView != null)
        {
            // Set up synchronization once for each parameter
            photonAnimatorView.SetParameterSynchronized("MoveX", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Continuous);
            photonAnimatorView.SetParameterSynchronized("MoveZ", PhotonAnimatorView.ParameterType.Float, PhotonAnimatorView.SynchronizeType.Continuous);
            
        }
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
        UpdateAnimatorParameters();
        HandleADS();
    }

    private void UpdateInput()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Update sprinting status: only allow sprinting if moving forward (input.y > 0)
        sprinting = (input.y > 0 && Input.GetKey(KeyCode.LeftShift));  // Assuming Left Shift is the sprint key

        // Toggle crouching
        if (Input.GetKeyDown(KeyCode.C)) // Assuming 'C' is the crouch button
        {
            crouching = !crouching; // Toggle crouch state
        }
    }

    private void HandleADS()
    {
        if (Input.GetButtonDown("Fire2") && !sprinting) // Assuming Fire2 is mapped to the ADS button
        {
            isAiming = true;
        }
        else if (Input.GetButtonUp("Fire2") || sprinting)
        {
            isAiming = false;
        }
    }

    private float DetermineMoveSpeed()
    {
        // If crouching and grounded, use crouch speed; if aiming, use ADS speed
        if (crouching && grounded && !sprinting) return crouchSpeed;
        if (isAiming) return ADSSpeed; 

        // Only allow sprinting if moving forward; otherwise, use walk speed
        if (sprinting && input.y > 0)
        {
            return sprintSpeed;
        }
        else
        {
            return walkSpeed;
        }
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

        // Set isMoving to true if the player is moving and not sprinting, crouching, or aiming
        //isMoving = input.magnitude > 0 && !sprinting && !crouching && !isAiming;

        // Update Animator parameters
        //UpdateAnimatorParameters();
    }

    private void HandleGroundMovement()
    {
        if (jumping)
            moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);

        float targetSpeed = moveSpeed;
        moveSpeed = Mathf.SmoothDamp(moveSpeed, targetSpeed, ref velocitySmoothing.x, input.magnitude > 0.5f ? accelerationTime : decelerationTime);

        // Calculate movement direction in world space based on player input
        Vector3 move = transform.right * input.x + transform.forward * input.y;
        moveDirection.x = move.x * moveSpeed;
        moveDirection.z = move.z * moveSpeed;

        // Convert moveDirection to local space (relative to player orientation)
        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);

        // Update Animator parameters for movement based on local movement direction
        //UpdateAnimatorParameters(localMoveDirection.x, localMoveDirection.z);
}


    private void HandleAirMovement()
    {
        moveDirection.x *= airDrag;
        moveDirection.z *= airDrag;
        moveDirection.y += Physics.gravity.y * Time.deltaTime;
    }

    private void AdjustCharacterHeight()
    {
        // Determine the target camera position based on crouch state
        float targetCameraY = crouching ? crouchHeight : originalCameraPosition.y;
        Vector3 targetCameraPosition = new Vector3(originalCameraPosition.x, targetCameraY, originalCameraPosition.z);

        // Smoothly transition the camera position
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetCameraPosition, Time.deltaTime * crouchTransitionSpeed);
    }

    [PunRPC]
    private void UpdateAnimatorParameters()
    {
        Vector3 localMoveDirection = transform.InverseTransformDirection(moveDirection);
        float scaledMoveX = localMoveDirection.x * 2.0f;
        float scaledMoveZ = localMoveDirection.z * 2.0f;

        isMoving = localMoveDirection.magnitude > 0 && !sprinting && !crouching && !isAiming;

        // Update animator parameters
        fpAnimator.SetFloat("MoveX", scaledMoveX);
        fpAnimator.SetFloat("MoveZ", scaledMoveZ);
        fpAnimator.SetBool("isMoving", isMoving);
        fpAnimator.SetBool("isAiming", isAiming);
        fpAnimator.SetBool("isSprinting", sprinting);
        fpAnimator.SetBool("isCrouching", crouching);

        // For third-person view animator (TP_model)
        tpAnimator.SetFloat("MoveX", scaledMoveX);
        tpAnimator.SetFloat("MoveZ", scaledMoveZ);
        tpAnimator.SetBool("isMoving", isMoving);
        tpAnimator.SetBool("isAiming", isAiming);
        tpAnimator.SetBool("isSprinting", sprinting);
        tpAnimator.SetBool("isCrouching", crouching);

        // Trigger RPCs for other clients to update their animation parameters
        photonView.RPC("UpdateRemoteAnimatorParameters", RpcTarget.Others, scaledMoveX, scaledMoveZ, isMoving, isAiming, sprinting, crouching);
    }

    [PunRPC]
    private void UpdateRemoteAnimatorParameters(float moveX, float moveZ, bool moving, bool aiming, bool sprint, bool crouch)
    {
        // Only for third-person model (remote players)
        tpAnimator.SetFloat("MoveX", moveX);
        tpAnimator.SetFloat("MoveZ", moveZ);
        tpAnimator.SetBool("isMoving", moving);
        tpAnimator.SetBool("isAiming", aiming);
        tpAnimator.SetBool("isSprinting", sprint);
        tpAnimator.SetBool("isCrouching", crouch);

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
