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
    // Access components of player object
    private Animator anim;
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;

    // Shut off user input at death/goal
    public bool ignoreUserInput = false;

    // Movement and jump variables
    private float horizontal;
    private float moveSpeed = 8f;
    private float jumpingPower = 16f;
    private bool isFacingRight = true;

    // Wall sliding variables
    private bool isWallSliding = false;
    private float wallSlidingSpeed = 2f;

    // Wall jumping variables
    private bool isWallJumping = false;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    // Enum of movement state animations for our player to cycle through.
    // Each variable equals      0     1        2        3        4        5            mathematically.
    private enum MovementState { idle, running, jumping, falling, dashing, wallSliding }
    MovementState state;
    

    // Start is called before the first frame update.
    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Cast enum state into int state
        anim.SetInteger("state", (int)state);

        if (ignoreUserInput)
        {
            rb.bodyType = RigidbodyType2D.Static;
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        UpdateAnimationState();
        WallSlide();
        WallJump();

        if (!isWallJumping)
        {
            Flip();
            rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        }
    }

    // Check if player is touching jumpable ground
    private bool IsGrounded()
    {
        // Create invisible circle at player's feet to check for overlap with jumpable ground
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // Check if player is touching a wall
    private bool IsWalled()
    {
        // Create invisible circle at player side to check for overlap with walls
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    // Check if player can wall slide and do it if so
    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    // Check if player can wall jump and do it if so
    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            Invoke(nameof(StopWallJumping), wallJumpingDuration);

            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = rb.transform.localScale;
                localScale.x *= -1f;
                rb.transform.localScale = localScale;
            }
        }
    }

    // Stop allowing player to wall jump
    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    // Flip the player when they move in that direction
    private void Flip()
    {
        // If facing right and moving left OR If facing left and moving right THEN flip
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector2 localScale = rb.transform.localScale;
            localScale.x *= -1f;
            rb.transform.localScale = localScale;
        }
    }

    // Switch between player animations based on movement
    private void UpdateAnimationState()
    {
        bool flipped = horizontal < 0f;

        // If moving right (positive x-axis) set running animation to true.
        if (horizontal > 0.1f && isFacingRight)
        {
            //isFacingRight = true;
            state = MovementState.running;  // running animation = true
            Flip();
        }
        // If moving left (negative x-axis) set running animation to true and flip animation on the x-axis.
        else if (horizontal < 0.1f && !isFacingRight)
        {
            //isFacingRight = false;
            state = MovementState.running;  // running animation = true
            Flip();
        }
        // If not moving set running animation to false.
        else
        {
            state = MovementState.idle;     // running animation = false
        }

        // We use +/-0.1f because our y-axis velocity is never perfectly zero.
        // If moving up (positive y-axis) set jumping animation to true.
        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jumping;  // jumping animation = true.
        }
        // If moving down (negative y-axis) set falling animation to true.
        else if (rb.velocity.y < -0.1f)
        {
            state = MovementState.falling;  // falling animation = true.
        }
    }
}
