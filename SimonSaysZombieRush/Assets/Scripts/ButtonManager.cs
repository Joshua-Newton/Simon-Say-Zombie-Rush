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
        int currentBuildIndex = SceneUtility.GetBuildIndexByScenePath(SceneManager.GetActiveScene().path);
        if (currentBuildIndex < SceneManager.sceneCountInBuildSettings - 1)
        {
            SceneManager.LoadSceneAsync(currentBuildIndex + 1);
            GameManager.instance.StateUnpause();
        }
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadSceneAsync(0);
        // TODO: New function to unpause while leaving cursor visible?
        GameManager.instance.StateUnpause();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void Respawn()
    {
        GameManager.instance.playerScript.SpawnPlayer();
        GameManager.instance.StateUnpause();
    }

}
