using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEngulfment : MonoBehaviour
{
    // Create relevant instances.
    private PlayerMovement pm;
    private HealthManager healthManager;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Slime Boss variables.
    private bool Engulfed = false;
    [SerializeField] private GameObject player;
    [SerializeField] private float damageToPlayer = 10f;

    // Start() is called before the first frame update.
    void Start()
    {
        // Access components once to save processing power.
        pm = player.GetComponent<PlayerMovement>();
        healthManager = player.GetComponent<HealthManager>();
    }

    // Update() is called once per frame.
    void Update()
    {
        // Update PlayerMovement script whether or not the player is engulfed.
        if (Engulfed) 
        {
            pm.isSlowed = true;
        }
        else
        { 
            pm.isSlowed = false; 
        }
    }

    // Player takes damage while engulfed.
    private void OnTriggerEnter2D(Collider2D col)
    {
        Engulfed = true;
        healthManager.TakeDamage(damageToPlayer);
    }

    // Disable engulfed.
    private void OnTriggerExit2D(Collider2D collision)
    {
        Engulfed = false;
    }
}