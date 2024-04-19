using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossScript : Enemy
{
    [SerializeField] private int bitCount;

    private float timeSinceLastJump = 2f;
    private float jumpForce = 25f;
    private float jumpFrequency = 2.5f;
    private bool facingLeft = true;
    private GameObject playerObject;
    [SerializeField] private GameObject bomb;
    private Rigidbody2D rb;
    private float turnDistance = 2f;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    protected override void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
        rb = bomb.GetComponent<Rigidbody2D>();
        spriteRenderer = transform.parent.GetComponent<SpriteRenderer>();
    }
    private new void Update()
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

        //change direction of boss
        //if player is right of boss and boss is facing left, then look right OR
        //if player is left of boss and boss is facing right, then look left
        if (((playerObject.transform.position.x > transform.position.x + turnDistance) && facingLeft) ||
            ((playerObject.transform.position.x < transform.position.x - turnDistance) && !facingLeft))
        {
            facingLeft = !facingLeft;
            Vector2 newDirection = rb.transform.localScale;
            newDirection.x *= -1;
            rb.transform.localScale = newDirection;
        }
    }

    private void FixedUpdate()
    {
        base.Update();

        if (timeSinceLastJump > jumpFrequency) 
        {
            Vector3 playerPosition = PlayerMovement.Instance.transform.position;
            Vector3 myPosition = transform.position;
            Vector2 direction = (playerPosition - myPosition);
                
            rb.velocity = new Vector2(rb.velocity.x + (direction.x), rb.velocity.y + jumpForce);
            timeSinceLastJump = 0;
        }

        timeSinceLastJump += Time.deltaTime;
    }

    //This function is called when the boss gets hit
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }

        timeSinceLastJump = 100; //force the boss to jump

        StartCoroutine(hitFlash(0.25f, spriteRenderer.color));
        
    }

    IEnumerator hitFlash(float length, Color originalColor)
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(length);
        spriteRenderer.color = originalColor;
    }

    private void OnDestroy()
    {
        SceneManager.LoadScene("Victory Screen");
    }
}
