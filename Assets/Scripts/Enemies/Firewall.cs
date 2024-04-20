using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firewall : MonoBehaviour
{
    private GameObject player;
    private HealthManager healthManager;
    [SerializeField] private float FirewallDamage = 10f;
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
            Debug.Log("Player detected at Firewall!");
            healthManager.TakeDamage(FirewallDamage);
        }
    }
}
