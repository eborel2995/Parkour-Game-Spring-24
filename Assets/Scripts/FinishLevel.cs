using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{
    // Variable to hold the audio source of the finish sound effect.
    //private AudioSource finishSound;

    // Variable to confirm whether player has completed level.
    // This also prevents the repeated playing of the finish sound effect.
    private bool levelCompleted = false;

    // Start is called before the first frame update.
    private void Start()
    {
        // Store GetComponent<AudioSource>() in finishSound to save memory and CPU resources.
        //finishSound = GetComponent<AudioSource>();
    }

    // Detect if player touches finish line.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If player collides with object AND level has not been completed, return true.
        if (collision.gameObject.name == "Player" && !levelCompleted)
        {
            // Play finish sound effect.
            //finishSound.Play();

            // Set levelCompleted to true after reaching finish line.
            levelCompleted = true;

            // Call CompleteLevel() method with delay.
            Invoke("CompleteLevel", 2f);
        }
    }

    // Method for completing a level.
    private void CompleteLevel()
    {
        // Load next level.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
