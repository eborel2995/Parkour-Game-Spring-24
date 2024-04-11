using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;

public class PlayerMovement : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    public bool ignoreUserInput = false;

    private float horizontal;
    private float moveSpeed = 8f;
    private float jumpingPower = 16f;

    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    // Enum of movement state animations for our player to cycle through.
    // Each variable equals      0     1        2        3        mathematically.
    private enum MovementState { idle, walking, jumping, falling }

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
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

        WallSlide();
        UpdateAnimationState();
    }

    //...

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
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

    //...
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

    //...
    private void UpdateAnimationState()
    {
        MovementState state;

        // If moving right (positive x-axis) set running animation to true.
        if (horizontal > 0f)
        {
            state = MovementState.walking;  // running animation = true
            sprite.flipX = false;   // flip animation to face right
        }
        // If moving left (negative x-axis) set running animation to true and flip animation on the x-axis.
        else if (horizontal < 0f)
        {
            state = MovementState.walking;  // running animation = true
            sprite.flipX = true;    // flip animation to face left
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































    /*
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

    }
    */
}
