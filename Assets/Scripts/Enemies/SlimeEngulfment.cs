using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEngulfment : MonoBehaviour
{
    //Note: this is old code for the old slime boss, but still used in Zip Bomber
    [SerializeField] private GameObject player;
    private PlayerMovement pm;
    private HealthManager healthManager;
    [SerializeField] private float damageToPlayer = 10f;

    private bool Engulfed = false;
    // Start is called before the first frame update
    void Start()
    {
        healthManager = player.GetComponent<HealthManager>();
        pm = player.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        //keep the PlayerMovement script updated on whether or not the player is engulfed
        if (Engulfed) 
            { pm.isEngulfed = true; }
        else{ pm.isEngulfed = false; }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //slow player while overlapping
        Engulfed = true;

        healthManager.TakeDamage(damageToPlayer);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //return player to regular speed
        Engulfed = false;
    }
}
