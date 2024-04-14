using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    protected PlayerMovement player;
    [SerializeField] protected float moveSpeed = 5f;
    
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator anim;
    

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
        
    }

}
