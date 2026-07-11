using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene switching

public class nextScene : MonoBehaviour
{
    public float delayInSeconds = 14.0f;

    void Start()
    {
        // loadin next scene after this number of seconds (put in Unity, default is 5)
        Invoke("LoadNextScene", delayInSeconds);
    }

    void LoadNextScene()
    {
        // Gets the current scene's index and adds 1 to go to the next one
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        // Check if there is a next scene available in Build Settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}