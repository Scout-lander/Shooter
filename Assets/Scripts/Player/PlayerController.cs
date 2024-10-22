using Photon.Pun;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviourPun
{
    public Movement movement;
    public PlayerClass playerClass;  // Add reference to the ScriptableObject
    private Skill[] currentSkills;
    private bool isSprinting;
    private bool isCrouching;
    private bool isJumping;
    private bool isSliding;

    [Header("Player States")]
    public bool canMove = true;

    void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        // Initialize the class data (assign model, weapons, and skills)
        InitializeClass(playerClass);
    }

     void InitializeClass(PlayerClass chosenClass)
    {
        // Set movement stats
        movement.SetMovementStats(chosenClass);
        // Assign skills
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
        // Handle skill usage or other class-specific actions
        HandleSkills();
    }
    void HandleSkills()
    {
        // Example for handling skill activation (could be skill 1, 2, 3...)
        if (Input.GetButtonDown("Skill1") && currentSkills.Length > 0)
        {
            UseSkill(currentSkills[0]);
        }
    }
    void UseSkill(Skill skill)
    {
        if (skill != null)
        {
            // Handle skill logic (e.g., cooldown, mana cost, etc.)
            Instantiate(skill.skillEffectPrefab, transform.position, Quaternion.identity); // Example of skill activation
        }
    }

    void HandleInput()
    {
        isSprinting = Input.GetButton("Sprint");
        isCrouching = Input.GetButton("Crouch");
        isJumping = Input.GetButton("Jump");

        if (isSprinting && isCrouching && movement.grounded && !isSliding)
        {
            StartSlide();
        }
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

    void StartSlide()
    {
        isSliding = true;
        movement.StartSlide();
        StartCoroutine(SlideCooldown(movement.slideDuration));
    }

    IEnumerator SlideCooldown(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndSlide();
    }

    void EndSlide()
    {
        isSliding = false;
        movement.EndSlide();
    }
}
