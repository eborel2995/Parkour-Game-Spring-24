using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDamage : MonoBehaviour
{
    private GameObject player;
    private HealthManager healthManager;
    [SerializeField] private float damagePerParticle = 1f;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        healthManager = player.GetComponent<HealthManager>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other == player)
        {
            Debug.Log("Player hit by damaging particles!");
            healthManager.TakeDamage(damagePerParticle);
        }
    }
}
