using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Cheats : MonoBehaviour
{
    [SerializeField] Vector3 resize;
    [SerializeField] float gravity;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gravity = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = resize;
        rb.gravityScale = gravity;


        if (Input.GetKeyDown(KeyCode.C))
        {
            
        }
        
        //fix issue of losing rb when scene changes!

    }
}
