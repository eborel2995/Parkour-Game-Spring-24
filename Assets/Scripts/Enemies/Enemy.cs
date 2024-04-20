using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float damage = 10f;

    [SerializeField] protected bool isRecoiling = false;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    protected float recoilTimer;

    [SerializeField] protected float moveSpeed = 5f;

    protected PlayerMovement player;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;
    protected HealthManager healthManager;

    
    protected virtual void Start()
    {
        //leave this empty so enemy types can override it
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        player = PlayerMovement.Instance;
    }
    // Update is called once per frame
    protected virtual void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
        }
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
    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;
        if (!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !PlayerMovement.Instance.pState.invincible)
        {
            EnemyAttack(collision.gameObject);
            PlayerMovement.Instance.HitStopTime(0.25f, 5, 0.5f);
        }
    }
    protected virtual void EnemyAttack(GameObject entity)
    {
        //PlayerMovement.Instance.TakeDamage(damage);

        //if the collision is with a player that is not invincible, and the player is alive
        if (entity.gameObject.CompareTag("Player")
            && !PlayerMovement.Instance.pState.invincible
            && PlayerMovement.Instance.pState.alive
            )
        {
            healthManager = entity.gameObject.GetComponent<HealthManager>();
            Debug.Log($"Enemy.cs: {gameObject.name} hit {entity.gameObject.name} for {damage}!");
            healthManager.TakeDamage(damage);
        }
    }
}
