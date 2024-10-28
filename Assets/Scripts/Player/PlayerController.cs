using Photon.Pun;
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviourPun
{
    public Movement movement;
    public PlayerClass playerClass;
    private Skill[] currentSkills;
    private bool isSprinting;
    private bool isCrouching;
    private bool isJumping;
    private bool isSliding;

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

        // Initialize the class data (assign model, weapons, and skills)
        InitializeClass(playerClass);
    }

    void InitializeClass(PlayerClass chosenClass)
    {
        movement.SetMovementStats(chosenClass);
        currentSkills = chosenClass.skills;

        // Additional setup for abilities, UI, etc.
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
            skill.Activate();  // Assuming the Skill class has an Activate method to handle logic
            Instantiate(skill.skillEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    void HandleInput()
    {
        // Check sprint input only if there's enough stamina and stamina is above zero
        if (movement.currentStamina > 0)
        {
            isSprinting = Input.GetButton("Sprint");
        }
        else
        {
            isSprinting = false; // Stop sprinting if stamina is zero
        }

        isCrouching = Input.GetButton("Crouch");
        isJumping = Input.GetButton("Jump");
    }

    void HandleMovement()
    {
        movement.sprinting = isSprinting;
        movement.crouching = isCrouching;
        movement.jumping = isJumping;

        if (!isSliding)
        {
            movement.MovePlayer();
        }
    }
}
