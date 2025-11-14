using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Text scoreText;
    public Text bestText;
    public Text levelText;
    public Button restartButton;
    public Button continueButton;       // NEW
    public GameObject gameOverPanel;
    public Text perfectText;

    void Awake()
    {
        Instance = this;
        if (perfectText != null)
            perfectText.gameObject.SetActive(false);
    }

    void Start()
    {
        UpdateBest();
        HideGameOver();
    }

    // ---------------------- PERFECT TEXT ------------------------

    public void ShowPerfectText()
    {
        if (perfectText == null) return;
        StartCoroutine(ShowPerfectRoutine());
    }

    private IEnumerator ShowPerfectRoutine()
    {
        perfectText.gameObject.SetActive(true);

        float duration = 1f;
        float elapsed = 0f;
        CanvasGroup cg = perfectText.GetComponent<CanvasGroup>();
        if (cg == null) cg = perfectText.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        perfectText.gameObject.SetActive(false);
    }

    // ---------------------- SCORE & LEVEL ------------------------

    public void UpdateScore(int score)
    {
        if (scoreText == null)
            return;

        scoreText.text = "Score: " + score;
    }

    public void UpdateLevel(int level)
    {
        if (levelText != null)
            levelText.text = "lvl: " + level;
    }

    void UpdateBest()
    {
        if (bestText != null)
            bestText.text = "Best: " + LeaderboardManager.Instance.GetBest();
    }

    // ---------------------- GAME OVER PANEL ------------------------

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
    }

    // ----------------------- BUTTONS -----------------------------

    public void OnRestartButton()
    {
        GameManager.Instance.Restart();
    }

    public void OnContinueButton()      // NEW
    {
        GameManager.Instance.ContinueGame();
    }
}
