using Photon.Pun;
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviourPun
{
    public Movement movement;
    public PlayerClass playerClass;
    public PlayerDash playerDash;
    private Skill[] currentSkills;
    public bool isSprinting;
    private bool isCrouching;
    private bool isJumping;
    private bool isDashing;

    [Header("Player States")]
    public bool canMove = true;
    public bool isADS = false;

    void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (movement == null)
        {
            Debug.LogError("Movement script is not assigned to PlayerController.");
            return;
        }
        if (playerDash == null)
        {
            Debug.LogError("PlayerDash script is not assigned to PlayerController.");
            return;
        }

        // Initialize the class data (assign model, weapons, and skills)
        InitializeClass(playerClass);
    }

    void InitializeClass(PlayerClass chosenClass)
    {
        movement.SetMovementStats(chosenClass);
        currentSkills = chosenClass.skills;

        // Additional setup for abilities, UI, etc.
    }
    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if (photonView.IsMine)
        {
            InitializeClass(playerClass);
        }
    }

    void Update()
    {
        if (canMove)
        {
            HandleInput();
            HandleMovement();
        }

        HandleSkills();
    }

    void HandleSkills()
    {
        if (Input.GetButtonDown("Skill1") && currentSkills.Length > 0 && currentSkills[0].IsReady())
        {
            UseSkill(currentSkills[0]);
        }
    }

    void UseSkill(Skill skill)
{
    if (skill != null)
    {
        skill.Activate();
        Instantiate(skill.skillEffectPrefab, transform.position, Quaternion.identity);
        //UpdateSkillUI(skill);  // Placeholder method to update cooldown UI
    }
}

    void HandleInput()
    {
        isSprinting = Input.GetButton("Sprint") && movement.currentStamina > 0;
        isCrouching = Input.GetButton("Crouch") && !isSprinting;
        isJumping = Input.GetButton("Jump");
    }

    void HandleMovement()
    {
        movement.sprinting = isSprinting;
        movement.crouching = isCrouching;
        movement.jumping = isJumping;
    }
    void HandleDash()
    {
        if (Input.GetMouseButtonDown(2) && playerDash != null && !isSprinting && !isJumping && playerDash.CanDash())
        {
            playerDash.PerformDash();
        }
    }
}
