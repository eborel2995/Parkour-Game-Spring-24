using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : Enemy
{
    const string LEFT = "left";
    const string RIGHT = "right";

    [SerializeField] protected Transform castPos;
    [SerializeField] protected float baseCastDist;

    string facingDirection;

    Vector3 baseScale;

    // Start is called before the first frame update
    protected override void Start()
    {
        baseScale = transform.localScale;

        facingDirection = RIGHT;
        
    }
    protected override void Awake()
    {
        base.Awake();
    }

    protected void FixedUpdate()
    {
        base.Update();
        float vX = moveSpeed;

        if(facingDirection == LEFT)
        {
            vX = -moveSpeed;
        }

        //move the game object
        rb.velocity = new Vector2(vX, rb.velocity.y);

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

    protected bool IsHittingWall()
    {
        bool val = false;

        float castDist = baseCastDist;

        // define the cast distance for left and right
        if(facingDirection == LEFT)
        {
            castDist = -baseCastDist;
        }

        // determine the target destination based on the cst distance
        Vector3 targetPos = castPos.position;
        targetPos.x += castDist;

        Debug.DrawLine(castPos.position, targetPos, Color.blue);

        if(Physics2D.Linecast(castPos.position, targetPos, 1 << LayerMask.NameToLayer("Ground")))
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
        bool val = true;

        float castDist = baseCastDist;


        // determine the target destination based on the cst distance
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
    public override void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damageDone, _hitDirection, _hitForce);
    }
}
