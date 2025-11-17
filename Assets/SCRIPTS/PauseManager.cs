using UnityEngine;
using UnityEngine.SceneManagement; // for exit
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;   // assign in inspector
    public Button pauseButton;
    public Button resumeButton;
    public Button exitButton;

    public static bool IsPaused { get; private set; } = false;

    //void Start()
    //{
    //    if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
    //    if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
    //    if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

    //    if (pauseMenuPanel != null)
    //        pauseMenuPanel.SetActive(false);

    //    Time.timeScale = 1f; // ensure unpaused start
    //    IsPaused = false;
    //}

    void Awake()
    {
        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        IsPaused = false;
    }


    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f; // ⏸️ stops physics & animations
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f; // ▶️ resume game normal time
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (pauseButton != null) pauseButton.gameObject.SetActive(true);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene("menu");
    }
}