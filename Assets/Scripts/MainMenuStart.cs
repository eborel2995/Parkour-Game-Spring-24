using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuStart : MonoBehaviour
{
    // Method to start the game.
    public void StartGame()
    {
        SceneManager.LoadScene("Level 1");
    }
    
    // Exit the game.
    public void QuitGame()
    {
        Application.Quit();
    }
}
