using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;         // Variable to hold our 2D rigid body (player). 
    private float dirX = 0f;        // Variable to hold direction on the x-axis (initialized to zero).

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    [SerializeField] private float moveSpeed = 7f;          // Variable to hold the movement speed of the player.
    [SerializeField] private float jumpForce = 14f;         // Variable to hold the jump force of the player.

    // Start is called before the first frame update.
    private void Start()
    {
        // Store GetComponent<Rigidbody2D>() once in rb to save memory and CPU resources.
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame.
    private void Update()
    {
        // Get direction on x-axis from Input Manager in Unity and store in dirX.
        // "Raw" in "GetAxisRaw" makes the player stop instantly when letting go of a directional key.
        dirX = Input.GetAxisRaw("Horizontal");

        // Use dirX to create velocity on the x-axis (joy-stick compatible).
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        // If space is pressed and IsGround() method returns true, then player jumps.
        if (Input.GetButtonDown("Jump") /*&& IsGrounded()*/)
        {
            // Play the jump sound effect.
            //jumpSoundEffect.Play();

            // Velocity for each axis: x = velocity from previous movement and y = 14.
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }
}
