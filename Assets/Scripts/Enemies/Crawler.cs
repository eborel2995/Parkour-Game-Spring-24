using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : Enemy
{
    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Crawler variables.
    Vector3 baseScale;
    const string LEFT = "left";
    const string RIGHT = "right";
    string facingDirection;
    [SerializeField] protected Transform castPos;
    [SerializeField] protected float baseCastDist; 

    // Start() is called before the first frame update.
    protected override void Start()
    {
        baseScale = transform.localScale;
        facingDirection = RIGHT;
    }

    // Awake() is called when the script instance is being loaded.
    // Awake() is used to initialize any variables or game states before the game starts.
    protected override void Awake()
    {
        base.Awake();
    }

    // FixedUpdate() can run once, zero, or several times per frame, depending on
    // how many physics frames per second are set in the time settings, and how
    // fast/slow the framerate is.
    protected void FixedUpdate()
    {
        base.Update();
        float vX = moveSpeed;

        // Update move speed with direction being faced.
        if(facingDirection == LEFT)
        {
            vX = -moveSpeed;
        }

        // Move the crawler.
        rb.velocity = new Vector2(vX, rb.velocity.y);

        // Make crawler turn around at edges and walls.
        if (IsHittingWall() || IsNearEdge())
        {
            if(facingDirection == LEFT)
            {
                ChangeFacingDirection(RIGHT);
            }
            else if(facingDirection == RIGHT)
            {
                ChangeFacingDirection(LEFT);
            }
        }
    }

    // Handles changing crawlers direction.
    protected void ChangeFacingDirection(string newDirection)
    {
        Vector3 newScale = baseScale;

        if(newDirection == LEFT)
        {
            newScale.x = -baseScale.x;
        }
        else if (newDirection == RIGHT) 
        {
            newScale.x = baseScale.x;
        }

        transform.localScale = newScale;
        facingDirection = newDirection;
    }

    // Check if crawler is hitting a wall.
    protected bool IsHittingWall()
    {
        bool val = false;
        float castDist = baseCastDist;

        // Define the cast distance for left and right.
        if(facingDirection == LEFT)
        {
            castDist = -baseCastDist;
        }

        // Determine the target destination based on the cast distance.
        Vector3 targetPos = castPos.position;
        targetPos.x += castDist;

        Debug.DrawLine(castPos.position, targetPos, Color.blue);

        // Get the layer masks for both "Ground" and "Wall".
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

    // Check if crawler is near an edge.
    protected bool IsNearEdge()
    {
        bool val = true;
        float castDist = baseCastDist;

        // Determine the target destination based on the cst distance.
        Vector3 targetPos = castPos.position;
        targetPos.y -= castDist;

        Debug.DrawLine(castPos.position, targetPos, Color.red);

        if (Physics2D.Linecast(castPos.position, targetPos, 1 << LayerMask.NameToLayer("Ground")))
        {
            val = false;
        }
        else
        {
            val = true;
        }

        return val;
    }

    // Handles crawler hit to player.
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    }
}