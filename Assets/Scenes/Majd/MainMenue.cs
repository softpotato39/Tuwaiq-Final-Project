using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject audioPanel;
    public GameObject displayPanel;
    public GameObject accessibilityPanel;
    public GameObject controlsPanel;

    // Music
    public AudioSource musicSource;

    public void PlayGame()
    {
        SceneManager.LoadScene("ALPHA");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // فتح الإعدادات
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        audioPanel.SetActive(false);
        displayPanel.SetActive(false);
        accessibilityPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    // فتح Audio
    public void OpenAudio()
    {
        settingsPanel.SetActive(false);
        audioPanel.SetActive(true);
    }

    // رجوع من Audio إلى Settings
    public void CloseAudio()
    {
        audioPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // فتح Display
    public void OpenDisplay()
    {
        settingsPanel.SetActive(false);
        displayPanel.SetActive(true);
    }

    // رجوع من Display إلى Settings
    public void CloseDisplay()
    {
        displayPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // فتح Accessibility
    public void OpenAccessibility()
    {
        settingsPanel.SetActive(false);
        accessibilityPanel.SetActive(true);
    }

    // رجوع من Accessibility إلى Settings
    public void CloseAccessibility()
    {
        accessibilityPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // فتح Controls
    public void OpenControls()
    {
        settingsPanel.SetActive(false);
        controlsPanel.SetActive(true);
    }

    // رجوع من Controls إلى Settings
    public void CloseControls()
    {
        controlsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // رجوع من Settings إلى Main Menu
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Fullscreen
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // Quality (واجهة فقط حالياً)
    public void SetQuality(int qualityIndex)
    {
        Debug.Log("Selected Quality: " + qualityIndex);
    }

    // Resolution
    public void SetResolution(int resolutionIndex)
    {
        switch (resolutionIndex)
        {
            case 0:
                Screen.SetResolution(1920, 1080, Screen.fullScreen);
                break;

            case 1:
                Screen.SetResolution(1600, 900, Screen.fullScreen);
                break;

            case 2:
                Screen.SetResolution(1280, 720, Screen.fullScreen);
                break;
        }

        Debug.Log("Width: " + Screen.width +
                  " Height: " + Screen.height);
    }

    // Subtitle Language
    public void SetSubtitleLanguage(int languageIndex)
    {
        Debug.Log("Language: " + languageIndex);
    }

    // Subtitle Size
    public void SetSubtitleSize(int sizeIndex)
    {
        Debug.Log("Subtitle Size: " + sizeIndex);
    }

    // Music Volume
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
}