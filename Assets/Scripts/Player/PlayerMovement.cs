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
    // Create an Instance of PlayerMovement to reference in other scripts.
    public static PlayerMovement Instance { get; private set; }

    // Class of bools that handle player states like jumping, dashing, and direction.
    [HideInInspector] public PlayerStatesList pState;

    // Sets player health and handles player health loss.
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;

    // Enum of movement state animations for our player to cycle through.
    // Each variable equals      0     1             2            3        4        5        6          mathematically.
    private enum MovementState { idle, runningRight, runningLeft, jumping, falling, dashing, wallSliding }
    MovementState state;

    // Access components for player object.
    private Animator anim;
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Shut off user input at death/goal.
    [Header("Player Settings:")]
    [SerializeField] public bool ignoreUserInput = false;

    // Ground check transform and layer for player jumping.
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    // Wall check transform and layer for player wall-jumping.
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [Space(5)]

    // Movement and jump variables.
    private bool isFacingRight = true;
    private float vertical;
    private float horizontal;
    private bool canDoubleJump = false;

    [Header("Player Movement Settings:")]
    private static float defaultMoveSpeed = 10f;
    [SerializeField] private float moveSpeed = defaultMoveSpeed;
    [SerializeField] private float jumpingPower = 21f;
    [SerializeField] private float gravity;

    // Wall sliding variables.
    private bool isWallSliding = false;
    [SerializeField] private float wallSlidingSpeed = 3f;

    // Wall jumping variables.
    private bool isWallJumping = false;
    private float wallJumpingCounter;
    private float wallJumpingDirection;
    private Vector2 wallJumpingPower = new Vector2(10f, 20f);
    [SerializeField] private float wallJumpingTime = 0.2f;
    [SerializeField] private float wallJumpingDuration = 0.4f;

    // Dashing variables.
    private bool canDash = true;
    private bool isDashing;
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 0.75f;
    [Space(5)]

    // AttackAttacking variables.
    // Handles player attack permissions and holds attack key from Unity Input Manager.
    bool attack = false;
    bool restoreTime;
    float timeSinceAttack;
    float restoreTimeSpeed;
    float timeBetweenAttack;
    [Header("Player Attack Settings:")]
    [SerializeField] float damage;

    // Attack transform, attack area, attackable layers, and attack animations.
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] GameObject slashEffect;
    [Space(5)]

    // Player attack recoil variables.
    private int stepsXRecoiled;
    private int stepsYRecoiled;
    [Header("Attack Recoil Settings:")]
    [SerializeField] int recoilXSteps = 3;
    [SerializeField] int recoilYSteps = 3;
    [SerializeField] float recoilXSpeed = 25;
    [SerializeField] float recoilYSpeed = 25;
    [Space(5)]

    // Player health variables.
    [Header("Player Health Settings:")]
    public int health;
    public int maxHealth;
    [SerializeField] float hitFlashSpeed;
    [SerializeField] GameObject bloodSpurt;

    [Header("Slime Boss Settings:")]
    public bool isEngulfed = false;
    private float engulfSlowRatio = 0.5f;

    public int Health
    {
        // Get player health.
        get { return health; }

        // Set player health.
        set
        {
            // Limit player health to a maxHealth value.
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                // If player health is changed then update player health value.
                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }

    // Awake() is called when the script instance is being loaded.
    // Awake is used to initialize any variables or game states before the game starts.
    private void Awake()
    { 

        // Error checking for the PlayerMovement instance.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy game object if instance is inaccessible.
        }
        // If no PlayerMovement instance is accessible then destroy game object.
        else
        {
            Instance = this;
        }

        // Set player health.
        Health = maxHealth;
    }

    // Start() is called before the first frame update.
    private void Start()
    {
        // Access components once to save processing power.
        pState = GetComponent<PlayerStatesList>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        gravity = rb.gravityScale;
    }

    // Draw three red rectangles for player melee attack area.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    // Update() is called once per frame
    void Update()
    {
        // Cast enum state into int state.
        anim.SetInteger("state", (int)state);

        // Make player static when ignoreUserInput is true.
        if (ignoreUserInput)
        {
            rb.bodyType = RigidbodyType2D.Static;
            return;
        }

        if (isEngulfed)
        {
            moveSpeed = defaultMoveSpeed * engulfSlowRatio;
            anim.speed = engulfSlowRatio;
        }
        else
        {
            moveSpeed = defaultMoveSpeed;
            anim.speed = 1.0f;
        }

        // Prevent player from moving, jumping, and flipping while dashing.
        if (isDashing) { return; }

        // Set attack, vertical, and horizontal input via Unity Input Manager.
        attack = Input.GetButtonDown("Attack");
        vertical = Input.GetAxisRaw("Vertical");
        horizontal = Input.GetAxisRaw("Horizontal");
        
        // Reset jump counter
        if (IsGrounded())
        {
            canDoubleJump = true;
        }

        // Jump if on jumpable ground or the single double jump.
        if (Input.GetButtonDown("Jump") && canDoubleJump)
        {
            if (!IsGrounded())
            { canDoubleJump = false; }

            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }
        // Letting go of jump will reduce the jump height
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // Dash by hitting leftShift if canDash is true.
        if (Input.GetButtonDown("Dash") && canDash && !isWallSliding)
        {
            StartCoroutine(Dash());
        }

        // Function calls.
        Attack();
        RestoreTimeScale();
        FlashWhileInvincible();
        WallSlide();
        WallJump();
        UpdateAnimationState();

        // Flip player direction when not wall jumping.
        if (!isWallJumping) { Flip(); }
    }

    // FixedUpdate() can run once, zero, or several times per frame, depending on
    // how mnay physics frames per second are set in the time settings, and how
    // fast/slow the framerate is.
    private void FixedUpdate()
    {
        // Prevent player from moving, jumping, and flipping while dashing.
        if (isDashing) { return; }

        // Get horizontal movement when not wall jumping.
        if (!isWallJumping)
        {
            rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);
        }

        // Apply recoil to attacks and damage.
        Recoil();
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

    // Handles player dashing permissions and execution.
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

    // Player attack handler.
    void Attack()
    {
        // Set the time since the last attack.
        timeSinceAttack = Time.deltaTime;

        // If the player can attack.
        if (attack && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;    // Reset time since last attack.
            anim.SetTrigger("Attacking");   // Set attacking animation trigger.

            try
            {
                // If player is on the ground then side attack and display slash effect.
                if (vertical == 0 || vertical < 0 && IsGrounded())
                {
                    Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
                    Instantiate(slashEffect, SideAttackTransform);
                }
                // If player's vertical input > 0 then up attack and display slash effect.
                else if (vertical > 0)
                {
                    Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
                    SlashEffectAtAngle(slashEffect, 80, UpAttackTransform);
                }
                // If player's vertical input < 0 then down attack and display slash effect.
                else if (vertical < 0 && !IsGrounded())
                {
                    Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
                    SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"Attacked entity has no ridigbody!");
            }
        }
    }

    // Handles player hit and recoil on enemies.
    private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        // Holds an array of colliders for objects that can be hit.
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);

        // If there is an object to hit then recoil.
        if (objectsToHit.Length > 0)
        {
            _recoilDir = true;
        }
        
        // Loop through objectsToHit array and deal damage accordingly.
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            // If enemy game object has not been destroyed.
            if (objectsToHit[i].GetComponent<Enemy>() != null)
            {
                // Apply hit damage to enemy, decrement enemy health, and recoil player.
                objectsToHit[i].GetComponent<Enemy>().EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
            }
        }
    }

    // Handles slash animations according to direction.
    void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        // Create slash effect.
        _slashEffect = Instantiate(_slashEffect, _attackTransform);

        // Handle slash effect positioning.
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);

        // Handle slash effect scale.
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }

    // Handles player recoil.
    void Recoil()
    {
        // If player is recoiling on the x-axis.
        if (pState.recoilingX)
        {
            // If facing right.
            if (isFacingRight)
            {
                // Apply left recoil to player hit.
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            // If facing left.
            else
            {
                // Apply right recoil to player hit.
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }

        // If player is recoiling on the y-axis.
        if (pState.recoilingY)
        {
            // Disable player gravity.
            rb.gravityScale = 0;

            // If player vertical input < 0.
            if (vertical < 0)
            {
                // Apply upward recoil to player hit.
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            // If player vertical input > 0.
            else
            {
                // Apply downward recoil to player hit.
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
        }
        // If player is not recoiling.
        else
        {
            // Set player gravity to original gravity value.
            rb.gravityScale = gravity;
        }

        // If player is recoiling on the x-axis AND player still has steps left to recoil.
        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            // Increment steps recoiled.
            stepsXRecoiled++;
        }
        // If player is out of steps to recoil then stop recoiling.
        else
        {
            StopRecoilX();
        }

        // If player is recoiling on the y-axis AND player still has steps left to recoil.
        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            // Increment steps recoiled.
            stepsYRecoiled++;
        }
        // If player is out of steps to recoil then stop recoiling.
        else
        {
            StopRecoilY();
        }

        // If player is touching jumpable ground then stop recoiling.
        if (IsGrounded())
        {
            StopRecoilY();
        }
    }

    // Stop player from recoiling on the x-axis.
    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }

    // Stop player from recoiling on the y-axis.
    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    // Handles player taking damage.
    public void TakeDamage(float _damage)
    {
        // Decrement player health.
        Health -= Mathf.RoundToInt(_damage);

        // Stop taking damage (invincibility-frames).
        StartCoroutine(StopTakingDamage());
    }

    // Handles player invincibility-frames and TakeDamage animation.
    IEnumerator StopTakingDamage()
    {
        // Enable player invincibility-frames.
        pState.invincible = true;

        // Create bloodspurt effect.
        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);

        // Destroy bloodspurt effect.
        Destroy(_bloodSpurtParticles, 1.5f);

        // Set TakeDamage animation trigger.
        anim.SetTrigger("TakeDamage");

        // Delay disabling invincibility-frames.
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    // Cause player sprite to flash when player is invincible.
    void FlashWhileInvincible()
    {
        if (sprite == null)
        {
            Debug.LogError("SpriteRenderer not assigned in PlayerMovement.");
            return; // Exit if sprite is null to prevent further errors
        }

        if (pState == null)
        {
            Debug.LogError("PlayerStatesList not assigned in PlayerMovement.");
            return; // Exit if pState is null to prevent further errors
        }

        // Change player sprite color if player is invincible and don't change if not.
        sprite.material.color = pState.invincible ?
            Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f)) : Color.white;
    }

    // Restores time scale if it has been modified.
    public void RestoreTimeScale()
    {
        // If time scale needs to be restored.
        if (restoreTime)
        {
            // If time scale < 1 then summation time scale.
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.deltaTime * restoreTimeSpeed;
            }
            // Stop restoring time scale when time scale = 1.
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }

    // Initiates change in time scale based on when player takes damage.
    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        Time.timeScale = _newTimeScale;

        // Check if time scale has a delay.
        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        // If time scale has delay then restore time scale.
        else
        {
            restoreTime = true;
        }
    }

    // Handles delay times.
    IEnumerator StartTimeAgain(float _delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(_delay);
    }
}
