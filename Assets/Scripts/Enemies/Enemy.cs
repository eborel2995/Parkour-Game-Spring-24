using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Access relevant components.
    protected Animator anim;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected PlayerMovement player;
    protected HealthManager healthManager;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Enemy variables.
    protected float recoilTimer;
    [SerializeField] protected float health;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected bool isRecoiling = false;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected float moveSpeed = 5f;

    // Start() is called before the first frame update.
    protected virtual void Start()
    {
        // Leave this empty so enemy types can override it.
    }

    // Awake() is called when the script instance is being loaded.
    // Awake() is used to initialize any variables or game states before the game starts.
    protected virtual void Awake()
    {
        // Access components once to save processing power.
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        player = PlayerMovement.Instance;
    }

    // Update() is called once per frame.
    protected virtual void Update()
    {
        // If enemy health is zero then destroy enemy.
        if (health <= 0)
        {
            Destroy(gameObject);
        }

        // Handles enemy recoil.
        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    // Handles enemy hits to player.
    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        // Do damage.
        health -= _damageDone;

        // Add recoil.
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }

    // Handles enemy and player collision.
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        // If object is the player AND player is not invincible.
        if (collision.gameObject.CompareTag("Player") && !PlayerMovement.Instance.pState.invincible)
        {
            // Hit the player.
            EnemyAttack(collision.gameObject);
            PlayerMovement.Instance.HitStopTime(0.25f, 5, 0.5f);
        }
    }

    // Handles enemy attack.
    protected virtual void EnemyAttack(GameObject entity)
    {
        // If the collision is with a player that is not invincible and alive.
        if (entity.gameObject.CompareTag("Player")
            && !PlayerMovement.Instance.pState.invincible
            && PlayerMovement.Instance.pState.alive
            )
        {
            // Do damage.
            healthManager = entity.gameObject.GetComponent<HealthManager>();
            healthManager.TakeDamage(damage);
        }
    }
}