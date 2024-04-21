using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICollide : MonoBehaviour
{
    // Create instance of HealthManager.
    private HealthManager healthManager;
    
    // Start() is called before the first frame update.
    void Start()
    {
        // Access components once to save processing power.
        healthManager = GetComponent<HealthManager>();
    }

    // Handles player collision.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If player touches trap then they die.
        if (collision.gameObject.CompareTag("Trap"))
        {
            healthManager.TakeDamage(healthManager.healthAmount);
        }
    }
}