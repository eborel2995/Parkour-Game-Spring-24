using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : Enemy
{
    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Charger variables.
    Vector3 baseScale;
    string facingDirection;
    const string LEFT = "left";
    const string RIGHT = "right";

    private float lastJumpTime;
    private float lastDirectionChangeTime;
    private float directionChangeCooldown = 0.5f;

    [SerializeField] private float jumpForce;
    [SerializeField] protected Transform castPos;
    [SerializeField] protected float baseCastDist;
    [SerializeField] private float chargeDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float detectionDistance;
    [SerializeField] private float jumpCooldown = 1.0f;
    [SerializeField] private float chargeSpeedMultiplier;

    // Enum for charger states.
    protected enum ChargerStates { Charger_Idle, Charger_Surprised, Charger_Charge, Charger_Jumping }
    ChargerStates currentChargerState;

    // Start() is called before the first frame update.
    protected override void Start()
    {
        baseScale = transform.localScale;
        facingDirection = RIGHT;
        currentChargerState = ChargerStates.Charger_Idle;
        lastDirectionChangeTime = Time.time;
    }

    // Awake() is called when the script instance is being loaded.
    // Awake() is used to initialize any variables or game states before the game starts.
    protected override void Awake()
    {
        base.Awake();
    }

    // Update() is called once per frame.
    protected override void Update()
    {
        base.Update();

        // Switch between charger states.
        switch (currentChargerState)
        {
            case ChargerStates.Charger_Idle:
                DetectPlayer();
                break;
            case ChargerStates.Charger_Surprised:
                if (IsGrounded()) // Ensure it is grounded before attempting to jump
                    Jump();
                break;
            case ChargerStates.Charger_Jumping:
                if (rb.velocity.y <= 0 && IsGrounded())
                {
                    anim.SetBool("IsJumping", false);
                    ChangeState(ChargerStates.Charger_Charge);
                }
                break;
            case ChargerStates.Charger_Charge:
                ChargeTowardsPlayer();
                break;
        }
    }

    // FixedUpdate() can run once, zero, or several times per frame, depending on
    // how many physics frames per second are set in the time settings, and how
    // fast/slow the framerate is.
    protected void FixedUpdate()
    {
        // Ignore if charger is charging.
        if (currentChargerState == ChargerStates.Charger_Charge) { return; }

        // Ignore if charger is jumping.
        if (currentChargerState == ChargerStates.Charger_Jumping) { return; }

        // Move the charger.
        float vX = facingDirection == LEFT ? -moveSpeed : moveSpeed;
        rb.velocity = new Vector2(vX, rb.velocity.y);

        // Make charger turn around at edges and walls.
        if (IsHittingWall() || IsNearEdge())
        {
            if (Time.time > lastDirectionChangeTime + directionChangeCooldown)
            {
                if (facingDirection == LEFT)
                {
                    ChangeFacingDirection(RIGHT);
                }
                else if (facingDirection == RIGHT)
                {
                    ChangeFacingDirection(LEFT);
                }
                lastDirectionChangeTime = Time.time;
            }
        }
    }

    // Handles changing chargers direction.
    protected void ChangeFacingDirection(string newDirection)
    {
        if (facingDirection != newDirection)
        {
            Vector3 newScale = baseScale;
            newScale.x = newDirection == LEFT ? -baseScale.x : baseScale.x;
            transform.localScale = newScale;
            facingDirection = newDirection;
        }
    }

    // Check if charger is hitting a wall.
    protected bool IsHittingWall()
    {
        bool val;
        float castDist = baseCastDist;

        // Define the cast distance for left and right.
        if (facingDirection == LEFT)
        {
            castDist = -baseCastDist;
        }

        // Determine the target destination based on the cst distance.
        Vector3 targetPos = castPos.position;
        targetPos.x += castDist;

        Debug.DrawLine(castPos.position, targetPos, Color.blue);

        // Get the layer masks for both "Ground" and "Wall".
        int groundLayer = LayerMask.NameToLayer("Ground");
        int wallLayer = LayerMask.NameToLayer("Wall");
        int combinedLayerMask = (1 << groundLayer) | (1 << wallLayer);

        if (Physics2D.Linecast(castPos.position, targetPos, combinedLayerMask))
        {  val = true; }
        else
        { val = false; }

        return val;
    }

    // Check if charger is near an edge.
    protected bool IsNearEdge()
    {
        float forwardCheckDistance = 0.2f; // Distance forward to check for an edge.
        float downwardCheckDistance = 0.5f; // Downward distance to check for ground.

        // Cast positions forward depending on the facing direction.
        Vector3 forwardCheck = castPos.position + (facingDirection == RIGHT ? Vector3.right : Vector3.left) * forwardCheckDistance;
        Vector3 downwardCheck = forwardCheck + Vector3.down * downwardCheckDistance;

        Debug.DrawRay(forwardCheck, Vector3.down * downwardCheckDistance, Color.red);

        // If there is no ground forward and downward, consider it an edge.
        return !Physics2D.Raycast(forwardCheck, Vector2.down, downwardCheckDistance, whatIsGround);
    }

    // Check if charger is grounded.
    private bool IsGrounded()
    {
        float checkDistance = 0.5f; // Check distance below the character.
        RaycastHit2D hit = Physics2D.Raycast(castPos.position, Vector2.down, checkDistance, whatIsGround);
        Debug.DrawRay(castPos.position, Vector2.down * checkDistance, hit ? Color.green : Color.red);
        return hit.collider != null;
    }

    // Handles chargers jump.
    private void Jump()
    {
        // Check cooldown and grounded state.
        if (Time.time - lastJumpTime >= jumpCooldown && IsGrounded()) 
        {
            anim.SetBool("IsJumping", true);
            rb.velocity = Vector2.up * jumpForce;   // Directly use Vector2.up for clarity.
            lastJumpTime = Time.time;
            ChangeState(ChargerStates.Charger_Jumping);
        }
    }

    // Handles chargers player detection.
    private void DetectPlayer()
    {
        if (PlayerMovement.Instance == null)
        {
            Debug.LogWarning("Player instance is missing, aborting player detection.");
            return;
        }

        // Detection should be independent of jump cooldown.
        float distance = Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position);

        // Change charger state based on player detection.
        if (distance <= detectionDistance && !IsNearEdge() && Time.time - lastJumpTime >= jumpCooldown)
        {
            ChangeState(ChargerStates.Charger_Surprised);
        }
        else
        {
            ChangeState(ChargerStates.Charger_Idle);
        }
    }

    // Handles how charger charges to player after detection.
    private void ChargeTowardsPlayer()
    {
        if (PlayerMovement.Instance == null || !PlayerMovement.Instance.gameObject.activeInHierarchy)
        {
            ChangeState(ChargerStates.Charger_Idle);
            return;
        }

        Vector3 playerPosition = PlayerMovement.Instance.transform.position;
        Vector3 position = transform.position;
        Vector2 direction = (playerPosition - position).normalized;
        float distance = Vector2.Distance(position, playerPosition);

        // Check if charger is near an edge or within player detection distance.
        if (IsNearEdge() || distance > chargeDistance)
        {
            //Debug.Log($"Stopping charge due to edge proximity(IsNearEdge(): {IsNearEdge()})  or player out of range. Distance of: {distance}");
            ChangeState(ChargerStates.Charger_Idle);
            return;
        }

        // Change charger direction.
        if (direction.x > 0 && facingDirection == LEFT) { ChangeFacingDirection(RIGHT); }
        else if (direction.x < 0 && facingDirection == RIGHT) { ChangeFacingDirection(LEFT); }

        // Activate charging movement.
        rb.velocity = new Vector2(direction.x * moveSpeed * chargeSpeedMultiplier, rb.velocity.y);
    }

    // Changes charger state.
    protected void ChangeState(ChargerStates newState)
    {
        // Guard clause to prevent nesting.
        // If the new state is the same as the current, then don't change anything and skip this function. 
        if (currentChargerState == newState)
        { return; }

        // Handle exiting states.
        switch (currentChargerState)
        {
            case ChargerStates.Charger_Jumping:
                anim.SetBool("IsJumping", false); // Ensure jumping animation is turned off.
                break;
        }

        currentChargerState = newState;

        // Handle entering new states.
        switch (newState)
        {
            case ChargerStates.Charger_Charge:
                anim.SetBool("IsJumping", false); // Explicitly ensure this is off.
                anim.speed = chargeSpeedMultiplier; // Speed up animation.
                break;
            case ChargerStates.Charger_Jumping:
                anim.SetBool("IsJumping", true);
                break;
            default:
                anim.speed = 1; // Reset speed when not charging.
                break;
            }
        
    }

    // Handles charger being attacked.
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    }
}