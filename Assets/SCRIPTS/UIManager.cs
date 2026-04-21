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
    public Text perfectText;

    public static UIManager Instance;

    public GameObject LevelUpPanel;
    //public Text LevelUpText;


    public TextMeshProUGUI planetText;

    public Text scoreCountText;
    public Text bestScoreCountText;

    public Text planetText_arrive;

    public Image planetImage_arrive;
    public Sprite[] planetIcons;   // same order as planets[]

    private Coroutine pulseRoutine;


    public string[] planets = {
    "Mercury", "Venus", "Earth", "Mars", "Jupiter",
    "Saturn", "Uranus", "Neptune", "Pluto"
};


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

    public void ShowLevelUp(int level)
    {
        string planetName = planets[level - 2];
        planetText_arrive.text = planetName;
        if (planetIcons != null && planetIcons.Length > level)
            planetImage_arrive.sprite = planetIcons[level];
        LevelUpPanel.SetActive(true);
        CancelInvoke(nameof(HideLevelUp));
        Invoke(nameof(HideLevelUp), 2.2f);
    }

    void HideLevelUp()
    {
        LevelUpPanel.SetActive(false);
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

    private IEnumerator ComboPop()
    {
        comboText.transform.localScale = Vector3.one * 1.4f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * 8f;
            comboText.transform.localScale = Vector3.Lerp(Vector3.one * 1.4f, Vector3.one, t);
            yield return null;
        }
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

    public void UpdateScoreGameover(int score)
    {
        scoreCountText.text = score.ToString();
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

        //bestText.text = "Best: " + LeaderboardManager.Instance.GetBest();
    }


    public void OnRestartButton()
    {
        GameManager.Instance.Restart();
    }

    public void OnContinueButton()
    {
        if (UnityAdsManager.Instance == null)
        {
            Debug.LogWarning("RewardedAdsManager.Instance is null – continuing without ad.");
            GameManager.Instance.ContinueGame();
            return;
        }

        // Show rewarded ad; continue only if user actually watched
        UnityAdsManager.Instance.ShowRewarded(watched =>
        {
            if (watched)
            {
                GameManager.Instance.ContinueGame();
            }
            else
            {
                // Optional: show popup "Ad not available" or similar
                Debug.Log("Continue cancelled – ad not watched or not available.");
            }
        });
    }


    public void UpdatePlanet(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, planets.Length - 1);
        planetText.text = planets[index];
    }

}