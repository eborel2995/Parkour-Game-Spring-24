using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossScript : MonoBehaviour
{
    [SerializeField] private int bitCount;
    // For Idle Stage
    [Header("Idle")]
    [SerializeField] float idleMoveSpeed;
    [SerializeField] Vector2 idleMoveDirection;

    private Rigidbody2D rb;

    private float timeSinceLastJump = 2f;
    private float jumpForce = 20f;
    //private float moveForce = 10f;
    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (timeSinceLastJump > 3) 
        {
            var rand = Random.Range(0, 100);
            if ( rand > 95)
            {
                Vector3 playerPosition = PlayerMovement.Instance.transform.position;
                Vector3 myPosition = transform.position;
                Vector2 direction = (playerPosition - myPosition);

                rb.velocity = new Vector2(rb.velocity.x + (direction.x), rb.velocity.y + jumpForce);
                timeSinceLastJump = 0;
            }
        }

        timeSinceLastJump += Time.deltaTime;
    }
}
