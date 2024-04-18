using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEngulfment : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private PlayerMovement pm;
    private HealthManager healthManager;

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
        if (Engulfed) 
            { pm.isEngulfed = true; }
        else{ pm.isEngulfed = false; }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //slow player while overlapping
        Engulfed = true;

        healthManager.TakeDamage(25);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //return player to regular speed
        Engulfed = false;
    }
}