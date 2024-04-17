using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    protected PlayerMovement player;
    [SerializeField] protected float moveSpeed = 5f;

    [SerializeField] protected float damage;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;

    protected float recoilTimer;

    protected HealthManager healthManager;
    protected PlayerStatesList pState;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        player = PlayerMovement.Instance;
        anim = GetComponent<Animator>();
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
        if (collision.gameObject.CompareTag("Player") && !PlayerMovement.Instance.pState.invincible && pState.alive)
        {
            EnemyAttack(collision.gameObject);
            PlayerMovement.Instance.HitStopTime(0, 5, 0.5f);
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
            Debug.Log($"{gameObject.name} hit {entity.gameObject.name}!");
            healthManager.TakeDamage(damage);
        }
    }
}
