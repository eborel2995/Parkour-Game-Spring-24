using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : Enemy
{
    const string LEFT = "left";
    const string RIGHT = "right";

    [SerializeField] protected Transform castPos;
    [SerializeField] protected float baseCastDist;

    [SerializeField] private float chargeSpeedMultiplier;
    [SerializeField] private float chargeDistance;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask whatIsGround;

    string facingDirection;

    Vector3 baseScale;

    [SerializeField] private float detectionDistance;

    private float directionChangeCooldown = 0.5f; // 0.5 seconds cooldown
    private float lastDirectionChangeTime;

    [SerializeField] private float jumpCooldown = 1.0f;  // Cooldown period in seconds
    private float lastJumpTime;

    protected enum ChargerStates
    {
        Charger_Idle,
        Charger_Surprised,
        Charger_Charge,
        Charger_Jumping
    }
    ChargerStates currentChargerState;

    // Start is called before the first frame update
    protected override void Start()
    {
        baseScale = transform.localScale;
        facingDirection = RIGHT;
        currentChargerState = ChargerStates.Charger_Idle;
        lastDirectionChangeTime = Time.time;
        //Debug.Log("Initial Detection Distance: " + detectionDistance);
    }
    protected override void Awake()
    {
        base.Awake();
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        //Debug.Log($"Current State: {currentChargerState}, Y Velocity: {rb.velocity.y}, Grounded: {IsGrounded()}");

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

    protected void FixedUpdate()
    {
        if(currentChargerState == ChargerStates.Charger_Charge) {
            // Only update direction if not charging or add more specific conditions
            return;
        }

        // Prevent horizontal movement when jumping
        if (currentChargerState == ChargerStates.Charger_Jumping)
        {
            return;
        }

        float vX = facingDirection == LEFT ? -moveSpeed : moveSpeed;
        rb.velocity = new Vector2(vX, rb.velocity.y);

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

    protected bool IsHittingWall()
    {
        bool val = false;

        float castDist = baseCastDist;

        // define the cast distance for left and right
        if (facingDirection == LEFT)
        {
            castDist = -baseCastDist;
        }

        // determine the target destination based on the cst distance
        Vector3 targetPos = castPos.position;
        targetPos.x += castDist;

        Debug.DrawLine(castPos.position, targetPos, Color.blue);

        // Get the layer masks for both "Ground" and "wall"
        int groundLayer = LayerMask.NameToLayer("Ground");
        int wallLayer = LayerMask.NameToLayer("Wall");
        int combinedLayerMask = (1 << groundLayer) | (1 << wallLayer);

        if (Physics2D.Linecast(castPos.position, targetPos, combinedLayerMask))
        {
            val = true;
        }
        else
        {
            val = false;
        }

        return val;
    }

    protected bool IsNearEdge()
    {
        float forwardCheckDistance = 0.2f; // Distance forward to check for an edge
        float downwardCheckDistance = 0.5f; // Downward distance to check for ground

        // Cast positions forward depending on the facing direction
        Vector3 forwardCheck = castPos.position + (facingDirection == RIGHT ? Vector3.right : Vector3.left) * forwardCheckDistance;
        Vector3 downwardCheck = forwardCheck + Vector3.down * downwardCheckDistance;

        Debug.DrawRay(forwardCheck, Vector3.down * downwardCheckDistance, Color.red);

        // If there is no ground forward and downward, consider it an edge
        return !Physics2D.Raycast(forwardCheck, Vector2.down, downwardCheckDistance, whatIsGround);
    }
    private bool IsGrounded()
    {
        float checkDistance = 0.5f; // Check distance below the character
        RaycastHit2D hit = Physics2D.Raycast(castPos.position, Vector2.down, checkDistance, whatIsGround);
        Debug.DrawRay(castPos.position, Vector2.down * checkDistance, hit ? Color.green : Color.red);

        Debug.Log($"Raycast for ground check: {hit.collider != null}");
        return hit.collider != null;
    }

    private void Jump()
    {
        if (Time.time - lastJumpTime >= jumpCooldown && IsGrounded()) // Check cooldown and grounded state
        {
            anim.SetBool("IsJumping", true);
            rb.velocity = Vector2.up * jumpForce; // Directly use Vector2.up for clarity
            lastJumpTime = Time.time;
            ChangeState(ChargerStates.Charger_Jumping);
        }
    }

    private void DetectPlayer()
    {
        if (PlayerMovement.Instance == null)
        {
            Debug.LogWarning("Player instance is missing, aborting player detection.");
            return;
        }

        // Detection should be independent of jump cooldown.
        float distance = Vector3.Distance(transform.position, PlayerMovement.Instance.transform.position);
        //Debug.Log($"Checking player distance: {distance}, Detection Distance: {detectionDistance}");

        if (distance <= detectionDistance && !IsNearEdge() && Time.time - lastJumpTime >= jumpCooldown)
        {
            ChangeState(ChargerStates.Charger_Surprised);
        }
        else
        {
            //Debug.Log("Player out of detection range or near edge, staying Idle.");
            ChangeState(ChargerStates.Charger_Idle);
        }
    }
    private void ChargeTowardsPlayer()
    {
        if (PlayerMovement.Instance == null || !PlayerMovement.Instance.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Player instance is missing or inactive, stopping charge.");
            ChangeState(ChargerStates.Charger_Idle);
            return;
        }

        Vector3 playerPosition = PlayerMovement.Instance.transform.position;
        Vector3 position = transform.position;
        Vector2 direction = (playerPosition - position).normalized;
        float distance = Vector2.Distance(position, playerPosition);

        if (IsNearEdge() || distance > chargeDistance)
        {
            Debug.Log($"Stopping charge due to edge proximity(IsNearEdge(): {IsNearEdge()})  or player out of range. Distance of: {distance}");
            ChangeState(ChargerStates.Charger_Idle);
            return;
        }

        if (direction.x > 0 && facingDirection == LEFT)
            ChangeFacingDirection(RIGHT);
        else if (direction.x < 0 && facingDirection == RIGHT)
            ChangeFacingDirection(LEFT);

        rb.velocity = new Vector2(direction.x * moveSpeed * chargeSpeedMultiplier, rb.velocity.y);
    }

    protected void ChangeState(ChargerStates newState)
    {
        if (currentChargerState != newState)
        {
            Debug.Log($"Changing state from {currentChargerState} to {newState}");
            // Handle exiting states
            switch (currentChargerState)
            {
                case ChargerStates.Charger_Jumping:
                    anim.SetBool("IsJumping", false); // Ensure jumping animation is turned off
                    break;
                    // Other states if necessary
            }

            currentChargerState = newState;

            // Handle entering new states
            switch (newState)
            {
                case ChargerStates.Charger_Charge:
                    anim.SetBool("IsJumping", false); // Explicitly ensure this is off
                    anim.speed = chargeSpeedMultiplier; // Speed up animation
                    break;
                case ChargerStates.Charger_Jumping:
                    anim.SetBool("IsJumping", true);
                    break;
                default:
                    anim.speed = 1; // Reset speed when not charging
                    break;
            }
        }
    }
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    }
}
