using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEngulfment : MonoBehaviour
{
    // Create relevant instances.
    private GameObject player;
    private PlayerStatesList pState;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    // Boss variables.
    private bool Engulfed = false;
    [SerializeField] private float damageToPlayer = 10f;

    // Start() is called before the first frame update.
    void Start()
    {
        // Access components once to save processing power.
        player = GameObject.Find("Player");
        pState = player.GetComponent<PlayerStatesList>();
    }

    // Update() is called once per frame.
    void Update()
    {
        // Update PlayerStatesList script whether or not the player is engulfed.
        if (Engulfed) 
        {
            pState.slowed = true;
        }
        else
        {
            pState.slowed = false; 
        }
    }

    // Player takes damage while engulfed.
    private void OnTriggerEnter2D(Collider2D col)
    {
        Engulfed = true;
    }

    // Disable engulfed.
    private void OnTriggerExit2D(Collider2D collision)
    {
        Engulfed = false;
    }
}