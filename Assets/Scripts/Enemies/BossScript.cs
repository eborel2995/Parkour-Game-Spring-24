using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossScript : Enemy
{
    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Boss variables.
    private Color original;
    private bool facingLeft = true;
    private float recordedPlayerHP;
    private float turnDistance = 2f; // Distance player must move until boss turns to prevent instantaneous turning.
    private float timeSinceLastJump = 1f; // Initial jump timer.
    [SerializeField] private GameObject bomb; // Parent object that Zip Bomber is riding.
    [SerializeField] private float jumpForce = 35f;
    [SerializeField] private float jumpFrequency = 2f;

    // Access relevant components.
    private HealthManager hm;
    private GameObject playerObject;
    private SpriteRenderer spriteRenderer;

    // Identified in Unity Inspector.
    [SerializeField] private ParticleSystem whiteLaunchParticles;
    [SerializeField] private ParticleSystem yellowLaunchParticles;
    [SerializeField] private ParticleSystem crashLaunchParticles;

    // Particles.
    ParticleSystem.EmissionModule emissions; // Used to toggle particle emitters.

    // Start() is called before the first frame update.
    protected override void Start()
    {
        // Access components once to save processing power.
        rb = bomb.GetComponent<Rigidbody2D>();
        spriteRenderer = transform.parent.GetComponent<SpriteRenderer>();
        original = spriteRenderer.color;
        playerObject = GameObject.Find("Player");
        hm = playerObject.GetComponent<HealthManager>();
    }

    // Update() is called once per frame.
    private new void Update()
    {
        gravityModifier(); // Launch at regular gravity, crash down after apex of launch.
        changeDirection(); // Whether player is on left or right of boss.
        recordedPlayerHP = hm.healthAmount; // Used for determining victory or defeat
    }

    // FixedUpdate() can run once, zero, or several times per frame, depending on
    // how many physics frames per second are set in the time settings, and how
    // fast/slow the framerate is.
    private void FixedUpdate()
    {
        base.Update(); // Get Enemy.cs updates.
        launchOffGround(); // Main movement function.
        timeSinceLastJump += Time.deltaTime;
        toggleGroundEmissions();
    }

    // Handles boss being attacked.
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }

        timeSinceLastJump = 100; // Force the boss to jump.

        StartCoroutine(hitFlash(0.25f, original));
    }

    // Handles boss movement.
    private void launchOffGround()
    {
        if (timeSinceLastJump > jumpFrequency)
        {
            launchParticles();

            Vector3 playerPosition = playerObject.transform.position;
            Vector3 myPosition = transform.position;
            Vector2 direction = (playerPosition - myPosition);
            
            rb.velocity = new Vector2(rb.velocity.x + (direction.x), rb.velocity.y + jumpForce);
            timeSinceLastJump = 0;
        }
    }

    private void launchParticles()
    {
        // Play explosive particles at launch.
        emissions = whiteLaunchParticles.emission;
        emissions.enabled = true;

        emissions = yellowLaunchParticles.emission;
        emissions.enabled = true;

        // Stop the crashing explosive particles before launching.
        emissions = crashLaunchParticles.emission;
        emissions.enabled = false;
    }

    // Handles boss particle emissions on the ground.
    private void toggleGroundEmissions()
    {
        // If boss is on the ground.
        if (rb.velocity.y < 0 && rb.position.y < 1.25)
        {
            emissions = whiteLaunchParticles.emission;
            emissions.enabled = false;

            emissions = yellowLaunchParticles.emission;
            emissions.enabled = false;

            emissions = crashLaunchParticles.emission;
            emissions.enabled = true;
            crashLaunchParticles.Play();
        }
    }

    // If the boss is at the apex of its jump (plus slight delay), increase gravity to simulate ground slam.
    private void gravityModifier()
    {
        if (rb.velocity.y < -5)
        {
            rb.gravityScale = 100;
        }
        else
        {
            rb.gravityScale = 5;
        }
    }

    // Change the direction of the boss.
    private void changeDirection()
    {
        // If player is right of boss and boss is facing left, then look right OR
        // if player is left of boss and boss is facing right, then look left.
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

    // When the boss is hit, flash the bomb red to show that it got hit.
    IEnumerator hitFlash(float timeLength, Color originalColor)
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(timeLength);
        spriteRenderer.color = originalColor;
    }

    // Handles which screen to show when defeating boss or boss defeats player.
    private void OnDestroy()
    {
        // Stop timer.
        Timer.instance.EndTimer();

        if (recordedPlayerHP > 0)
        {
            SceneManager.LoadScene("Victory Screen");
        }
        else
        {
            SceneManager.LoadScene("GameOver");
        }
    }
}