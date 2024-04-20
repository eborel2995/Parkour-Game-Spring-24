using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class BossScript : Enemy
{
    private float timeSinceLastJump = 1f; //initial jump timer
    [SerializeField] private float jumpForce = 35f;
    [SerializeField] private float jumpFrequency = 2f;
    private bool facingLeft = true;
    [SerializeField] private GameObject bomb; //parent object that Zip Bomber is riding
    private float turnDistance = 2f; //distance player must move until boss turns to prevent instantaneous turning
    private SpriteRenderer spriteRenderer;
    private Color original;

    private GameObject playerObject;
    private HealthManager hm;
    private float recordedPlayerHP;

    //identified in Unity Inspector
    [SerializeField] private ParticleSystem whiteLaunchParticles;
    [SerializeField] private ParticleSystem yellowLaunchParticles;
    [SerializeField] private ParticleSystem crashLaunchParticles;

    //Particles
    ParticleSystem.EmissionModule emissions; //used to toggle particle emitters

    // Start is called before the first frame update
    protected override void Start()
    {
        rb = bomb.GetComponent<Rigidbody2D>();
        spriteRenderer = transform.parent.GetComponent<SpriteRenderer>();
        original = spriteRenderer.color;

        playerObject = GameObject.FindWithTag("Player");
        hm = playerObject.GetComponent<HealthManager>();
    }
    private new void Update()
    {
        gravityModifier(); //launch at regular gravity, crash down after apex of launch
        changeDirection(); //whether player is on left or right of boss
        recordedPlayerHP = hm.healthAmount;
    }

    private void FixedUpdate()
    {
        //get Enemy.cs updates
        base.Update();

        launchOffGround(); //main movement function
        
        timeSinceLastJump += Time.deltaTime;

        toggleGroundEmissions();
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

        StartCoroutine(hitFlash(0.25f, original));
    }

    //main helper function for boss movement
    private void launchOffGround()
    {
        if (timeSinceLastJump > jumpFrequency)
        {
            //play explosive particles at launch
            emissions = whiteLaunchParticles.emission;
            emissions.enabled = true;

            emissions = yellowLaunchParticles.emission;
            emissions.enabled = true;

            //stop the crashing explosive particles before launching
            emissions = crashLaunchParticles.emission;
            emissions.enabled = false;

            Vector3 playerPosition = PlayerMovement.Instance.transform.position;
            Vector3 myPosition = transform.position;
            Vector2 direction = (playerPosition - myPosition);

            rb.velocity = new Vector2(rb.velocity.x + (direction.x), rb.velocity.y + jumpForce);
            timeSinceLastJump = 0;
        }
    }

    private void toggleGroundEmissions()
    {
        //when boss is on the ground
        if (rb.velocity.y < 0)
        {
            emissions = whiteLaunchParticles.emission;
            emissions.enabled = false;

            emissions = yellowLaunchParticles.emission;
            emissions.enabled = false;

            emissions = crashLaunchParticles.emission;
            emissions.enabled = true;
        }
    }

    private void gravityModifier()
    {
        //if the boss is at the apex of its jump (plus slight delay), increase gravity to simulate ground slam
        if (rb.velocity.y < -5)
        {
            rb.gravityScale = 100;
        }
        else
        {
            rb.gravityScale = 5;
        }
    }

    private void changeDirection()
    {
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

            timeSinceLastJump = 0; //reset jump timer 
        }
    }

    IEnumerator hitFlash(float timeLength, Color originalColor)
    {
        //when the boss is hit, flash the bomb red to show that it got hit
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(timeLength);
        spriteRenderer.color = originalColor;
    }

    private void OnDestroy()
    {
        Timer.instance.EndTimer();
        //bug: player respawning resets entire scene, which destroys the boss, which triggers this even when the player dies
        if (recordedPlayerHP > 0)
        {
            SceneManager.LoadScene("Victory Screen");
        }
        else
        {
            SceneManager.LoadScene("GameOver");
        }
        //todo: victory particles
        //StartCoroutine(GoToVictoryScreen());
    }

    /*
    //doesn't work because the GameObject would be destroyed
    IEnumerator GoToVictoryScreen()
    {
        yield return new WaitForSeconds(5);
        //bug: player respawning resets entire scene, which destroys the boss, which triggers this even when the player dies
        SceneManager.LoadScene("Victory Screen");

    }
    */
}

