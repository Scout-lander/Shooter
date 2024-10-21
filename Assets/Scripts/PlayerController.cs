using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    public Movement movement;
    private bool isSprinting;
    private bool isCrouching;
    private bool isJumping;
    private bool isSliding;

    [Header("Player States")]
    public bool canMove = true;  // Flag to control when the player can move

    void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }
    }

    void Update()
    {
        if (canMove)
        {
            HandleInput();       // Handle input for movement
            HandleMovement();    // Call the Movement script to move the player
        }
    }

    void HandleInput()
    {
        // Handle player input for movement-related actions
        isSprinting = Input.GetButton("Sprint");
        isCrouching = Input.GetButton("Crouch");
        isJumping = Input.GetButton("Jump");

        // Check if the player can start sliding
        if (isSprinting && isCrouching && movement.grounded && !isSliding)
        {
            StartSlide();
        }
    }

    void HandleMovement()
    {
        // Pass movement states to the Movement script
        movement.sprinting = isSprinting;
        movement.crouching = isCrouching;
        movement.jumping = isJumping;

        if (!isSliding)
        {
            movement.MovePlayer();  // Call the movement logic
        }
    }

    void StartSlide()
    {
        // Handle sliding
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
