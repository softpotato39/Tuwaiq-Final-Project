using UnityEngine;

public class PauseSettings : MonoBehaviour
{
    public GameObject settingsPanel;

    public GameObject audioPanel;
    public GameObject displayPanel;
    public GameObject accessibilityPanel;
    public GameObject controlsPanel;

    void Start()
    {
        CloseAll();
    }

    void CloseAll()
    {
        audioPanel.SetActive(false);
        displayPanel.SetActive(false);
        accessibilityPanel.SetActive(false);
        controlsPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        CloseAll();
    }

    public void OpenAudio()
    {
        settingsPanel.SetActive(false);
        CloseAll();
        audioPanel.SetActive(true);
    }

    public void CloseAudio()
    {
        audioPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenDisplay()
    {
        settingsPanel.SetActive(false);
        CloseAll();
        displayPanel.SetActive(true);
    }

    public void CloseDisplay()
    {
        displayPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenAccessibility()
    {
        settingsPanel.SetActive(false);
        CloseAll();
        accessibilityPanel.SetActive(true);
    }

    public void CloseAccessibility()
    {
        accessibilityPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenControls()
    {
        settingsPanel.SetActive(false);
        CloseAll();
        controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        controlsPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        CloseAll();
        settingsPanel.SetActive(false);
    }
}