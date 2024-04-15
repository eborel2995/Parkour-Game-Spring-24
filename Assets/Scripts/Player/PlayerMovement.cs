using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Security.Cryptography;
using Unity.VisualScripting.ReorderableList;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Processors;
using UnityEngine.InputSystem.XR.Haptics;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;

public class PlayerMovement : MonoBehaviour
{
    // Access components for player object.
    private Animator anim;
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;

    // Shut off user input at death/goal.
    public bool ignoreUserInput = false;

    // Movement and jump variables.
    private bool isFacingRight = true;
    private float horizontal;
    private float moveSpeed = 10f;
    private float jumpingPower = 21f;
    private float gravity;

    // Wall sliding variables.
    private bool isWallSliding = false;
    private float wallSlidingSpeed = 3f;

    // Wall jumping variables.
    private bool isWallJumping = false;
    private float wallJumpingCounter;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(10f, 20f);

    // Dashing variables.
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 0.75f;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    // possiable attack vars and stuff
    [Header("Attacking Setting")]
    float timeBetweenAttack, timeSinceAttack;
    //for attack animations and attack area and attackable layers
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage;
    [SerializeField] GameObject slashEffect;
    bool restoreTime;
    float restoreTimeSpeed;
    [Space(5)]

    [Header("Recoil Settings")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    private int stepsXRecoiled, stepsYRecoiled;
    [Space(5)]

    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    [SerializeField] GameObject bloodSpurt;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;

    bool attack = false;
    private float xAxis, yAxis;
    // Create an Instance of PlayerMovement to reference in other scripts.
    public static PlayerMovement Instance { get; private set; }
    [HideInInspector] public PlayerStatesList pState;
    // Enum of movement state animations for our player to cycle through.
    // Each variable equals      0     1             2            3        4        5        6          mathematically.
    private enum MovementState { idle, runningRight, runningLeft, jumping, falling, dashing, wallSliding }
    MovementState state;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Optional: Only if you want the player to persist across scenes
        }
        else
        {
            Destroy(gameObject);  // Ensures there's only one instance of the PlayerMovement object
        }
        Health = maxHealth;
    }
    // Start() is called before the first frame update.
    private void Start()
    {
        // Access components once to save processing power.
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        gravity = rb.gravityScale;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }
    // Update() is called once per frame.
    private void Update()
    {
        // Cast enum state into int state.
        anim.SetInteger("state", (int)state);

        // Make player static when ignoreUserInput is true.
        if (ignoreUserInput)
        {
            rb.bodyType = RigidbodyType2D.Static;
            return;
        }

        // Prevent player from moving, jumping, and flipping while dashing.
        if (isDashing) { return; }

        // Set horizontal input via Unity Input Manager.
        horizontal = Input.GetAxisRaw("Horizontal");

        // Jump if on jumpable ground.
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }
        // Holding jump will let the player jump higher.
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // Dash by hitting leftShift if canDash is true.
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isWallSliding)
        {
            StartCoroutine(Dash());
        }

        UpdateAnimationState();
        WallSlide();
        WallJump();

        // Get horizontal movement when not wall jumping.
        if (!isWallJumping) { Flip(); }
        Attack();
        RestoreTimeScale();
        FlashWhileInvincible();
    }

    // FixedUpdate() can run once, zero, or several times per frame, depending on
    // how mnay physics frames per second are set in the time settings, and how
    // fast/slow the framerate is.
    private void FixedUpdate()
    {
        //if the player is temporarily Static, stop trying to do anything
        if (rb.bodyType == RigidbodyType2D.Static) {return;}

        // Prevent player from moving, jumping, and flipping while dashing.
        if (isDashing) { return; }

        // Get horizontal movement when not wall jumping.
        if (!isWallJumping)
        {
            rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        }
    }

    // Check if player is touching jumpable ground.
    private bool IsGrounded()
    {
        // Create invisible circle at player's feet to check for overlap with jumpable ground.
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // Check if player is touching a wall.
    private bool IsWalled()
    {
        // Create invisible circle at player side to check for overlap with walls.
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, wallLayer);
    }

    // Check if player can wall slide and do it if so.
    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;

            // Clamp player to wall and set wall slide speed.
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    // Check if player can wall jump and do it if so.
    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;

            // Wall jumping direction is the direction opposite of where the player is facing.
            wallJumpingDirection = -transform.localScale.x;

            // Set wall jumping counter.
            wallJumpingCounter = wallJumpingTime;

            // Stop invoking the method StopWallJumping().
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            // Decrement wall jumping counter.
            wallJumpingCounter -= Time.deltaTime;
        }

        // If the player jumps while wall sliding they will wall jump.
        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;

            // Apply wall jump physics.
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);

            // Reset wall jumping counter.
            wallJumpingCounter = 0f;

            // Invoke the method StopWallJumping().
            Invoke(nameof(StopWallJumping), wallJumpingDuration);

            // Check if player is facing the correct direction to wall jump off the next wall.
            // If not, change the direction the player is facing.
            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = rb.transform.localScale;
                localScale.x *= -1f;
                rb.transform.localScale = localScale;
            }
        }
    }

    // Stop allowing player to wall jump.
    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    // Flip the player when they move in that direction.
    private void Flip()
    {
        // If facing right and moving left OR If facing left and moving right THEN flip.
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector2 localScale = rb.transform.localScale;
            localScale.x *= -1f;
            rb.transform.localScale = localScale;
        }
    }

    // Dashing phyics, mechanics, and trail emitter.
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // Preserve original gravity value and set gravity to zero while dashing.
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Dashing physics.
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);

        // Dash for the as long as dashingTime.
        yield return new WaitForSeconds(dashingTime);

        // Restore gravity after dashing.
        rb.gravityScale = originalGravity;
        isDashing = false;
        
        // Player can dash again after dashingCooldown.
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    // Switch between player animations based on movement.
    private void UpdateAnimationState()
    {
        if (!isWallJumping)
        {
            // If not moving set state to idle animation.
            if (horizontal == 0f) { state = MovementState.idle; }

            // If moving right (positive x-axis) set state to runningRight animation.
            // *It just works with != instead of > so DO NOT change this*
            else if (horizontal != 0f) { state = MovementState.runningRight; }

            // If moving left (negative x-axis) set state to runningLeft animation.
            else if (horizontal < 0f) { state = MovementState.runningLeft; }

            // We use +/-0.1f because our y-axis velocity is rarely perfectly zero.
            // If moving up (positive y-axis) set state to jumping animation.
            if (rb.velocity.y > 0.1f) { state = MovementState.jumping; }

            // If moving down (negative y-axis) set state to falling animation.
            else if (rb.velocity.y < -0.1f) { state = MovementState.falling; }

            // If wall sliding set state to wallSliding animation.
            if (isWallSliding) { state = MovementState.wallSliding; }

            // If dashing set state to dashing animation.
            if (isDashing) { state = MovementState.dashing; }
        }
    }
    // how attacks are handled
    void Attack()
    {
        timeSinceAttack = Time.deltaTime;
        if (attack && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");

            if (yAxis == 0 || yAxis < 0 && IsGrounded())
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
                Instantiate(slashEffect, SideAttackTransform);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, 80, UpAttackTransform);
            }
            else if (yAxis < 0 && !IsGrounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);
            }
        }
    }
    //hit with recoil
    private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        if (objectsToHit.Length > 0)
        {
            _recoilDir = true;
        }
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            if (objectsToHit[i].GetComponent<Enemy2>() != null)
            {
                objectsToHit[i].GetComponent<Enemy2>().EnemyHit(damage,
                    (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
            }
        }
    }
    // slach animations
    void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }
    void Recoil()
    {
        //Debug.Log("Recoil from update");
        if (pState.recoilingX)
        {

            if (pState.lookingRight)
            {
                Debug.Log("If looking right");
                rb.velocity = new Vector2(-recoilXSpeed, 0);

            }
            else
            {
                Debug.Log("Looking Left");
                rb.velocity = new Vector2(recoilXSpeed, 0);

            }
        }
        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            //airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        //stop recoil
        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (IsGrounded())
        {
            StopRecoilY();
        }
    }
    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }
    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }
    //player taking damage 
    public void TakeDamage(float _damage)
    {
        Health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
    }
    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1.5f);
        anim.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(1f);
        pState.invincible = false;

    }
    public int Health
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }
    // flashing and invinciblity time from being attacked
    void FlashWhileInvincible()
    {
        sprite.material.color = pState.invincible ?
            Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f)) : Color.white;
    }
    void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.deltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }
    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        Time.timeScale = _newTimeScale;
        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }
    }
    IEnumerator StartTimeAgain(float _delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(_delay);
    }
}