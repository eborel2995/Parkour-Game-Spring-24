using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.ProBuilder.MeshOperations;

public class PlayerMovement : MonoBehaviour
{
    // Get access to the components of the current object (player) so we can modify them in code
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private BoxCollider2D coll;     
    private float dirX = 0f;        // Variable to hold direction on the x-axis (initialized to zero).

    public bool ignoreUserInput = false;

    private bool movingLeft = false;
    private bool movingRight = false;
    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    [SerializeField] private float moveSpeed;          
    [SerializeField] private float jumpForce;         
    [SerializeField] private LayerMask jumpableGround;      // Variable to check against IsGrounded() method.
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    /*
    // TODO: higher jump when holding jump longer
    private bool endedJumpEarly = true;
    private float currentTimeHoldingJump = 0f;
    private float fallSpeed = 5;
    private float jumpEndEarlyGravityModifer = 20;
    */

    //jump buffer
    //private float jumpBuffer = 0.1f;

    //coyote time
    /*
    private float timeLeftGround;
    private float coyoteTimeThreshhold = 0.1f;
    */

    //clamped fall speed
    [SerializeField] private float maxFallSpeed = -5;

    // Enum of movement state animations for our player to cycle through.
    // Each variable equals      0     1        2        3        mathematically.
    private enum MovementState { idle, walking, jumping, falling }

    private enum MovementDirection { left, right }

    // Start is called before the first frame update.
    private void Start()
    {
        // Store components once to save memory and CPU resources.
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame.
    private void Update()
    {

        // Get direction on x-axis from Input Manager in Unity and store in dirX.
        // "Raw" in "GetAxisRaw" makes the player stop instantly when letting go of a directional key.
        //dirX = Input.GetAxisRaw("Horizontal");

        //TODO: if holding shift, multiply moveSpeed by sprint multiplier

        
        if (ignoreUserInput) 
        {
            rb.bodyType = RigidbodyType2D.Static; 
            return;
        }

        //if player is pressing A or D then enable movement left or right
        if (Input.GetKeyDown(KeyCode.A))
        {
            movingLeft = true;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            movingRight = true;
        }

        //stop movement when player releases the key
        if (Input.GetKeyUp(KeyCode.A))
        {
            movingLeft = false;
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            movingRight = false;
        }
        
        //move the player
        if (movingLeft && !movingRight)
        {
            transform.position += (Vector3.left * moveSpeed) * Time.deltaTime;
        }
        else if (movingRight && !movingLeft)
        {
            transform.position += (Vector3.right * moveSpeed) * Time.deltaTime;
        }

        WallSlide();

        // Use dirX to create velocity on the x-axis (joy-stick compatible).
        //rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);


        //todo: jump input buffer 
        /*
        if (IsGrounded() && lastJumpPressed + jumpBuffer > Time.time)
        {
            jump();
        }
        */

        //todo: nudge player when they bump their head

        //if player is holding jump and on ground, then jump
        jump();
        
        if (rb.velocity.y < maxFallSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, maxFallSpeed);
        }

        UpdateAnimationState();

    }

    private void UpdateAnimationState()
    {
        MovementState state;

        // If moving right (positive x-axis) set running animation to true.
        if (dirX > 0f || movingRight)
        {
            state = MovementState.walking;  // running animation = true
            sprite.flipX = false;   // flip animation to face right
        }
        // If moving left (negative x-axis) set running animation to true and flip animation on the x-axis.
        else if (dirX < 0f || movingLeft)
        {
            state = MovementState.walking;  // running animation = true
            sprite.flipX = true;    // flip animatino to face left
        }
        // If not moving set running animation to false.
        else
        {
            state = MovementState.idle; // running animation = false
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

        // Cast enum state into int state
        anim.SetInteger("state", (int)state);
    }
    

    /// <summary>
    /// Create a box around the player model that is slightly lower than player's hitbox.
    /// </summary>
    /// <returns>
    /// If box overlaps jumpableGround return true, then player can jump again.
    /// If box does not overlap jumpableGround return false, then player cannot jump again.
    /// </returns>
    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }

    // is player touching a wall for wall sliding
    // Create a circle around the player that is slightly to the front of the where the player is facing.
    // If circle overlaps wallLayer return true, then player can wall slide.
    // If circle does not overlap wallLayer return false, then player cannot wall slide.
    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
        //return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.right, 0.2f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && dirX != 0f)
        {
            Debug.Log("I should be wallsliding right now");
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void jump()
    {
        //DateTime now = DateTime.Now;
        // If space is pressed and IsGround() method returns true, then player jumps.
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            //jumpSoundEffect.Play();


            //using AddForce causes an unintended effect of being able to climb walls by spamming jump on the wall
            //but its kinda fun and replicable so I'm gonna keep it for now
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            //rb.velocity = new Vector2(rb.velocity.x, jumpForce); //old jump code
        }

        //todo: jump higher / shorter based on how long player holds jump
        //if (Input.GetButtonUp("Jump") && ??? && rb.velocity.y > 0)
        //{
        //endedJumpEarly = true;


        //}

        /*
        endedJumpEarly && rb.velocity.y > 0
            ? fallSpeed * jumpEndEarlyGravityModifer
            : fallSpeed;
        
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - (fallSpeed * Time.deltaTime));
        */
    }
}
