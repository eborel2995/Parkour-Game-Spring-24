using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting.ReorderableList;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Processors;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;

public class PlayerMovement : MonoBehaviour
{
    // Access components for player object.
    private Animator anim;
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;

    // Shut off user input at death/goal.
    public bool ignoreUserInput = false;

    // Movement and jump variables.
    private bool isFacingRight = true;
    private float horizontal;
    private float moveSpeed = 10f;
    private float jumpingPower = 21f;

    // Wall sliding variables.
    private bool isWallSliding = false;
    private float wallSlidingSpeed = 3f;

    // Wall jumping variables.
    private bool isWallJumping = false;
    private float wallJumpingCounter;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(10f, 20f);

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    //...
    public static PlayerMovement Instance { get; private set; }

    // Enum of movement state animations for our player to cycle through.
    // Each variable equals      0     1             2            3        4        5        6          mathematically.
    private enum MovementState { idle, runningRight, runningLeft, jumping, falling, dashing, wallSliding }
    MovementState state;

    // Start is called before the first frame update.
    private void Start()
    {
        // Access components once to save processing power.
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame.
    private void Update()
    {
        // Cast enum state into int state.
        anim.SetInteger("state", (int)state);

        // Make player static when ignoreUserInput is true.
        if (ignoreUserInput)
        {
            rb.bodyType = RigidbodyType2D.Static;
            return;
        }

        // Set horizontal input via Unity Input Manager.
        horizontal = Input.GetAxisRaw("Horizontal");

        // Jump if on jumpable ground.
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }
        // Holding jump will let the player jump higher.
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        UpdateAnimationState();
        WallSlide();
        WallJump();

        // Get horizontal movement when not wall jumping.
        if (!isWallJumping)
        {
            Flip();
            rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        }
    }

    // Check if player is touching jumpable ground.
    private bool IsGrounded()
    {
        // Create invisible circle at player's feet to check for overlap with jumpable ground.
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // Check if player is touching a wall.
    private bool IsWalled()
    {
        // Create invisible circle at player side to check for overlap with walls.
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, wallLayer);
    }

    // Check if player can wall slide and do it if so.
    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;

            // Clamp player to wall and set wall slide speed.
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    // Check if player can wall jump and do it if so.
    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;

            // Wall jumping direction is the direction opposite of where the player is facing.
            wallJumpingDirection = -transform.localScale.x;

            // Set wall jumping counter.
            wallJumpingCounter = wallJumpingTime;

            // Stop invoking the method StopWallJumping().
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            // Decrement wall jumping counter.
            wallJumpingCounter -= Time.deltaTime;
        }

        // If the player jumps while wall sliding they will wall jump.
        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;

            // Apply wall jump physics.
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);

            // Reset wall jumping counter.
            wallJumpingCounter = 0f;

            // Invoke the method StopWallJumping().
            Invoke(nameof(StopWallJumping), wallJumpingDuration);

            // Check if player is facing the correct direction to wall jump off the next wall.
            // If not, change the direction the player is facing.
            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = rb.transform.localScale;
                localScale.x *= -1f;
                rb.transform.localScale = localScale;
            }
        }
    }

    // Stop allowing player to wall jump.
    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    // Flip the player when they move in that direction.
    private void Flip()
    {
        // If facing right and moving left OR If facing left and moving right THEN flip.
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector2 localScale = rb.transform.localScale;
            localScale.x *= -1f;
            rb.transform.localScale = localScale;
        }
    }

    // Switch between player animations based on movement.
    private void UpdateAnimationState()
    {
        if (!isWallJumping)
        {
            // If not moving set state to idle animation.
            if (horizontal == 0f)
            {
                state = MovementState.idle;
            }
            // If moving right (positive x-axis) set state to runningRight animation.
            // *It just works with != instead of > so DO NOT change this*
            else if (horizontal != 0f)
            {
                state = MovementState.runningRight;
            }
            // If moving left (negative x-axis) set state to runningLeft animation.
            else if (horizontal < 0f)
            {
                state = MovementState.runningLeft;
            }
            
            // We use +/-0.1f because our y-axis velocity is rarely perfectly zero.
            // If moving up (positive y-axis) set state to jumping animation.
            if (rb.velocity.y > 0.1f)
            {
                state = MovementState.jumping;
            }
            // If moving down (negative y-axis) set state to falling animation.
            else if (rb.velocity.y < -0.1f)
            {
                state = MovementState.falling;
            }

            // If wall sliding set state to wallSliding animation.
            if (isWallSliding)
            {
                state = MovementState.wallSliding;
            }
        }
    }
}