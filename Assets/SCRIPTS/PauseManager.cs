using UnityEngine;
using UnityEngine.SceneManagement; // for exit
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;   // assign in inspector
    public Button pauseButton;
    public Button resumeButton;
    public Button exitButton;

    private bool isPaused = false;

    void Start()
    {
        // Ensure buttons are wired
        pauseButton.onClick.AddListener(TogglePause);
        resumeButton.onClick.AddListener(ResumeGame);
        exitButton.onClick.AddListener(ExitGame);

        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; // make sure game starts unpaused
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // ⏸️ stops all Update & physics
        pauseMenuPanel.SetActive(true);
        pauseButton.gameObject.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // ▶️ resume normal time
        pauseMenuPanel.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f; // reset time
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // for builds
#endif
    }
}
