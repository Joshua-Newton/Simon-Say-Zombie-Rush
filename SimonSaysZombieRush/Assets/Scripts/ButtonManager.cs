using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public void Resume()
    {
        GameManager.instance.StateUnpause();
    }

    public void Restart()
    {
        // TODO: DO NOT do this in a shipped game.... but this is simple for the purposes of the course
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        GameManager.instance.StateUnpause();
    }

    public void Quit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void LoadNextLevel()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        int currentLevelNumber;

        // Extract the current level number from the scene name
        if (int.TryParse(currentSceneName.Replace("Level", ""), out currentLevelNumber))
        {
            int nextLevelNumber = currentLevelNumber + 1;
            string nextSceneName = "Level" + nextLevelNumber;

            // Check if the next level exists in the build settings
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("Next level not found: " + nextSceneName);
                // Handle what happens if there are no more levels
                // For example, you might want to show a game over screen or loop back to the first level
            }
        }
        else
        {
            Debug.LogError("Current scene name does not follow the 'LevelX' format: " + currentSceneName);
        }
    }


}
