using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuStart : MonoBehaviour
{
    // Method to start the game.
    public void StartGame()
    {
        // Load next scene (level 1).
        SceneManager.LoadScene("Level 1");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
