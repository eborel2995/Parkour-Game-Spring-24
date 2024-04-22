using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{
    // Level completion variables.
    private bool levelCompleted = false;
    private float sceneSwapDelayS = 1f;
    private GameObject player;

    // "[SerializeFeild]" allows these variables to be edited in Unity.
    [SerializeField] private string nextScene;

    private void Start()
    {
        player = GameObject.Find("Player");
    }

    // Detect if player touches finish line.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If player collides with object AND level has not been completed, return true.
        if (collision.gameObject == player && !levelCompleted)
        {
            //ignore user input so player stays at the goal
            player.GetComponent<PlayerMovement>().ignoreUserInput = true;

            // Set levelCompleted to true after reaching finish line.
            levelCompleted = true;

            // Call CompleteLevel() method with delay.
            Invoke("CompleteLevel", sceneSwapDelayS);
        }
    }

    // Method for completing a level.
    private void CompleteLevel()
    {
        // Load next level.
        SceneManager.LoadScene(nextScene);
    }
}