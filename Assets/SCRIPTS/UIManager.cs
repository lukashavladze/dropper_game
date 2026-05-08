using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class UIManager : MonoBehaviour
{
    public Text scoreText;
    public Text bestText;
    public Text levelText;
    public Button restartButton;
    public GameObject gameOverPanel;
    public TextMeshProUGUI perfectText;
    private Coroutine perfectRoutine;

    public static UIManager Instance;

    public GameObject LevelUpPanel;
    //public Text LevelUpText;


    public TextMeshProUGUI planetText;

    public Text scoreCountText;
    public Text bestScoreCountText;

    private Coroutine pulseRoutine;

    [Header("Level Progress")]
    public Image progressFill;
    public TextMeshProUGUI progressText;

    private float currentProgress = 0f;
    private Coroutine progressRoutine;


    [Header("Combo UI")]
    public TextMeshProUGUI comboText;
    public CanvasGroup comboCanvas;



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

    public void UpdateCombo(int combo, int multiplier)
    {
        if (comboText == null || comboCanvas == null) return;

        if (combo <= 1)
        {
            comboCanvas.alpha = 0f;

            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }

            return;
        }

        comboCanvas.alpha = 1f;
        comboText.text = multiplier + "x!";

        // color progression
        if (multiplier < 3)
            comboText.color = Color.yellow;
        else if (multiplier < 5)
            comboText.color = new Color(1f, 0.5f, 0f);
        else
            comboText.color = Color.red;

        // start pulsating if not already running
        if (pulseRoutine == null)
        {
            pulseRoutine = StartCoroutine(ComboPulse());
        }
    }

    IEnumerator ComboPulse()
    {
        while (true)
        {
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime * 4f;
                float scale = Mathf.Lerp(1f, 1.2f, Mathf.Sin(t * Mathf.PI));
                comboText.transform.localScale = Vector3.one * scale;
                yield return null;
            }
        }
    }


    public void UpdateProgress(float target)
    {
        if (progressFill == null) return;

        // clamp safety
        target = Mathf.Clamp01(target);

        // avoid unnecessary animation (tiny difference)
        if (Mathf.Abs(currentProgress - target) < 0.001f)
            return;

        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        progressRoutine = StartCoroutine(AnimateProgress(target));
    }

    IEnumerator AnimateProgress(float target)
    {
        float start = currentProgress;
        float duration = 0.25f; // smoother & consistent
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / duration;

            // smoother easing (feels better than linear)
            t = t * t * (3f - 2f * t); // SmoothStep

            currentProgress = Mathf.Lerp(start, target, t);
            progressFill.fillAmount = currentProgress;

            // optional text update
            if (progressText != null && GameManager.Instance != null)
            {
                progressText.text = $"{GameManager.Instance.GetBlocksPlaced()}/{GameManager.Instance.GetBlocksRequired()}";
            }

            yield return null;
        }

        currentProgress = target;
        progressFill.fillAmount = currentProgress;

        progressRoutine = null;
    }


    public void ShowPerfectText()
    {
        if (perfectText == null) return;

        if (perfectRoutine != null)
            StopCoroutine(perfectRoutine);

        perfectRoutine = StartCoroutine(ShowPerfectRoutine());
    }

    private IEnumerator ShowPerfectRoutine()
    {
        perfectText.gameObject.SetActive(true);

        CanvasGroup cg = perfectText.GetComponent<CanvasGroup>();

        if (cg == null)
            cg = perfectText.gameObject.AddComponent<CanvasGroup>();

        // RESET VISIBILITY
        cg.alpha = 1f;

        // RESET SCALE
        perfectText.transform.localScale = Vector3.one * 1.2f;

        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = elapsed / duration;

            cg.alpha = Mathf.Lerp(1f, 0f, t);

            perfectText.transform.localScale =
                Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, t);

            yield return null;
        }

        cg.alpha = 0f;
        perfectText.gameObject.SetActive(false);

        perfectRoutine = null;
    }


    public void UpdateScore(int score)
    {
        if (scoreText == null)
        {
            Debug.LogError("⚠️ UIManager: ScoreText is NOT assigned in the Inspector!");
            return;
        }

        scoreText.text = score.ToString();

        //if (score > LeaderboardManager.Instance.GetBest())
        //{
        //    LeaderboardManager.Instance.SetBest(score);
        //    UpdateBest();
        //}
    }

    public void UpdateScoreGameover(int score)
    {
        scoreCountText.text = score.ToString();
    }

    public void UpdateBestScoreGameover(int score)
    {
        bestScoreCountText.text = score.ToString();
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
        if (bestText == null)
        {
            Debug.LogWarning("UIManager: bestText is not assigned in the Inspector.");
            return;
        }

        if (LeaderboardManager.Instance == null)
        {
            Debug.LogWarning("UIManager: LeaderboardManager.Instance is null – best score will show as 0.");
            bestText.text = "Best: 0";
            return;
        }
    }


    public void OnRestartButton()
    {
        GameManager.Instance.Restart();
    }

    public void OnContinueButton()
    {
        if (AdManager.Instance == null)
        {
            GameManager.Instance.ContinueGame();
            return;
        }

        AdManager.Instance.ShowRewarded(success =>
        {
            if (success)
            {
                GameManager.Instance.ContinueGame();
            }
            else
            {
                Debug.Log("Ad not available");
            }
        });
    }

}