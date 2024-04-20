using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICollide : MonoBehaviour
{
    private HealthManager healthManager;
    
    // Start is called before the first frame update
    void Start()
    {
        healthManager = GetComponent<HealthManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If player touches trap, return true and play death animation.
        if (collision.gameObject.CompareTag("Trap"))
        {
            Debug.Log("fell into trap, die now");
            healthManager.TakeDamage(healthManager.healthAmount);
        }
    }
}
