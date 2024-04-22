using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICollide : MonoBehaviour
{
    private GameObject player;
    private HealthManager healthManager;
    [SerializeField] private float spikeDamage = 34f;
    [SerializeField] private float spikeBounceHeight = 10f;
    
    // Start() is called before the first frame update.
    void Start()
    {
        player = GameObject.Find("Player");
        healthManager = GetComponent<HealthManager>();
    }

    // Handles player collision.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If player touches trap then they die.
        if (collision.gameObject.CompareTag("Trap"))
        {
            healthManager.TakeDamage(spikeDamage);
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + spikeBounceHeight);

        }
    }
}