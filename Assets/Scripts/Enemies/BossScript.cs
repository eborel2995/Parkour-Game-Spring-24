using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossScript : Enemy
{
    [SerializeField] private int bitCount;

    private Rigidbody2D rb;

    private float timeSinceLastJump = 2f;
    private float jumpForce = 20f;

    // Start is called before the first frame update
    protected override void Start()
    {
    }

    private void Update()
    {
        //if the slime is at the apex of its jump (plus slight delay), increase gravity to simulate ground slam
        if (rb.velocity.y < -5)
        {
            rb.gravityScale = 100;
        }
        else
        {
            rb.gravityScale = 5;
        }
    }

    private void FixedUpdate()
    {
        base.Update();

        if (timeSinceLastJump > 3) 
        {
            //make it so there is a chance the slime doesn't jump
            var rand = Random.Range(0, 100);
            if ( rand > 95 )
            {
                Vector3 playerPosition = PlayerMovement.Instance.transform.position;
                Vector3 myPosition = transform.position;
                Vector2 direction = (playerPosition - myPosition);

                rb.velocity = new Vector2(rb.velocity.x + (direction.x), rb.velocity.y + jumpForce);
                timeSinceLastJump = 0;
            }
        }

        timeSinceLastJump += Time.deltaTime;
    }
}
