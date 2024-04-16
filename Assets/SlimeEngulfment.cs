using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEngulfment : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private PlayerMovement pm;
    private bool Engulfed = false;
    private HealthManager healthManager;
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
        Engulfed = true;

        healthManager.TakeDamage(20);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Engulfed = false;
    }
}
