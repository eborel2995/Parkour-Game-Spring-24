using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSlowsPlayer : MonoBehaviour
{
    // Create relevant instances.
    private GameObject player;
    private PlayerStatesList pState;

    // Start() is called before the first frame update.
    void Start()
    {
        // Access components once to save processing power.
        player = GameObject.Find("Player");
        pState = player.GetComponent<PlayerStatesList>();
    }

    // Player takes damage while engulfed.
    private void OnTriggerEnter2D(Collider2D col)
    {
        pState.slowed = true;
    }

    // Disable engulfed.
    private void OnTriggerExit2D(Collider2D collision)
    {
        pState.slowed = false;
    }
}