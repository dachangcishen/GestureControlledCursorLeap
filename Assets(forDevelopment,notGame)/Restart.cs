using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Import the SceneManagement namespace

public class Restart : MonoBehaviour
{
    // This method will be called when the button is clicked
    
    public void RestartScene()
    {
        // Reload the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void QuitGame()
    {
        // Quit the application
        Application.Quit();
        
        // If running in the editor, stop playing the scene
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
