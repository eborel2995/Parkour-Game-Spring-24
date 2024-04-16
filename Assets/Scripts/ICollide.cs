using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICollide : MonoBehaviour
{
    private HealthManager healthManager;
    private Cheats cheats;
    
    // Start is called before the first frame update
    void Start()
    {
        healthManager = GetComponent<HealthManager>();
        cheats = GetComponent<Cheats>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (cheats.invincible == true)
        { return;  }

        // If player touches trap, return true and play death animation.
        if (collision.gameObject.CompareTag("Trap"))
        {
            Debug.Log("fell into trap, die now");
            healthManager.TakeDamage(healthManager.healthAmount);
            healthManager.Die();  // Death animation.
            healthManager.Respawn();
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            healthManager.TakeDamage(20);
        }
    }
}
