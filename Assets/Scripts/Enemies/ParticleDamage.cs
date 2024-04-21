using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDamage : MonoBehaviour
{
    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Access relevant components.
    private GameObject player;
    private HealthManager healthManager;
    [SerializeField] private float damagePerParticle = 1f;

    // Start() is called before the first frame update.
    void Start()
    {
        // Access components once to save processing power.
        player = GameObject.Find("Player");
        healthManager = player.GetComponent<HealthManager>();
    }

    // Handles particle collision with player.
    private void OnParticleCollision(GameObject other)
    {
        if (other == player)
        {
            healthManager.TakeDamage(damagePerParticle);
        }
    }
}