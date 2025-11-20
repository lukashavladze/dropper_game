using System.Collections;
using UnityEngine;
using UnityEngine.UI;



public class UIManager : MonoBehaviour
{
    public Text scoreText;
    public Text bestText;
    public Text levelText;
    public Button restartButton;
    public GameObject gameOverPanel;
    public Text perfectText;

    public static UIManager Instance;

    public Text LevelUpText;



    void Start()
    {
        UpdateBest();
        HideGameOver();
    }

    void Awake()
    {
        Instance = this;
        if (perfectText != null)
            perfectText.gameObject.SetActive(false);
    }

    public void ShowLevelUp(int level)
    {
        LevelUpText.text = "LEVEL " + level;
        LevelUpText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideLevelUp));
        Invoke(nameof(HideLevelUp), 1.2f);
    }

    void HideLevelUp()
    {
        LevelUpText.gameObject.SetActive(false);
    }

    public void ShowPerfectText()
    {
        if (perfectText == null) return;
        StartCoroutine(ShowPerfectRoutine());
    }

    private IEnumerator ShowPerfectRoutine()
    {
        perfectText.gameObject.SetActive(true);
        perfectText.transform.localScale = Vector3.one * 1.2f;

        float duration = 1f;
        float elapsed = 0f;

        // Fade + scale down smoothly
        CanvasGroup cg = perfectText.GetComponent<CanvasGroup>();
        if (cg == null) cg = perfectText.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // unaffected by pause
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            perfectText.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, elapsed / duration);
            yield return null;
        }

        cg.alpha = 0f;
        perfectText.gameObject.SetActive(false);
    }


    public void UpdateScore(int score)
    {
        if (scoreText == null)
        {
            Debug.LogError("⚠️ UIManager: ScoreText is NOT assigned in the Inspector!");
            return;
        }

        scoreText.text = "Score: " + score;

        //if (score > LeaderboardManager.Instance.GetBest())
        //{
        //    LeaderboardManager.Instance.SetBest(score);
        //    UpdateBest();
        //}
    }


    public void UpdateLevel(int level)
    {
        levelText.text = "lvl: " + level;
    }


    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }
    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
    }


    void UpdateBest()
    {
        bestText.text = "Best: " + LeaderboardManager.Instance.GetBest();
    }


    public void OnRestartButton()
    {
        GameManager.Instance.Restart();
    }
}