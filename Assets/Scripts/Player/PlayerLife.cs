using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLife : MonoBehaviour
{
    // Get access to the components of the current object (player) so we can modify them in code
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovement pm;
    private Cheats cheats;
    private Respawn respawn;

    //Death sound effect
    [SerializeField] private AudioSource DeathSoundEffect;

    // Start is called before the first frame update
    void Start()
    {
        // Store components once to save memory and CPU resources.
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        pm = GetComponent<PlayerMovement>();
        cheats = GetComponent<Cheats>();
        respawn = GetComponent<Respawn>();
    }

    // Method for player colliding with traps.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If player touches trap, return true and play death animation.
        if (collision.gameObject.CompareTag("Trap") || collision.gameObject.CompareTag("Enemy"))
        {
            if (cheats.invincible == false)
            {
                Die();  // Death animation.
            }
            
        }
    }

    // Method to activate death animation and disable player movement.
    private void Die()
    {
        // Disable player movement.
        rb.bodyType = RigidbodyType2D.Static;
        pm.ignoreUserInput = true;

        // Activate death animation.
        anim.SetTrigger("death");

    }

    // Method to restart the level upon player's death.
    private void RestartLevel()
    {
        // Restart level.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
