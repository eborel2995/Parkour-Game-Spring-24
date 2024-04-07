using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    protected PlayerMovement player;
    [SerializeField] protected float moveSpeed = 5f;
    

    protected float recoilTimer;
    protected Rigidbody2D rb;

    protected enum EnemyStates
    {
        //Crawler
        Crawler_Idle,
        Crawler_Flip,
    }
    protected EnemyStates currentEnemyState;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerMovement.Instance;
    }
    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    protected virtual void UpdateEnemyStates() { }

    protected void ChangeState(EnemyStates _newState)
    {
        currentEnemyState = _newState;
    }

}
