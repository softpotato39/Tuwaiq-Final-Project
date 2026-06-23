using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
public GameObject settingsPanel;
    private bool isPaused = false;

    void Start()
    {
        

        pauseMenu.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

   void PauseGame()
{
    pauseMenu.SetActive(true);
    settingsPanel.SetActive(false);

    Time.timeScale = 0f;
    isPaused = true;

    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
}

    void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ResumeFromButton()
    {
        ResumeGame();
    }
    public void GoToMainMenu()
{
    Time.timeScale = 1f;
    SceneManager.LoadScene("MainMenu");
}
public void OpenSettings()
{
    Debug.Log("OPEN SETTINGS");

    settingsPanel.SetActive(true);
}
public void CloseSettings()
{
    settingsPanel.SetActive(false);
    pauseMenu.SetActive(true);
}
}