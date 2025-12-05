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


    public string[] planets = {
    "Mercury", "Venus", "Earth", "Mars", "Jupiter",
    "Saturn", "Uranus", "Neptune", "Pluto"
};



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
        string planetName = planets[level - 1];
        planetText_arrive.text = planetName;
        if (planetIcons != null && planetIcons.Length > level)
            planetImage_arrive.sprite = planetIcons[level - 1];
        LevelUpPanel.SetActive(true);
        CancelInvoke(nameof(HideLevelUp));
        Invoke(nameof(HideLevelUp), 2.2f);
    }

    void HideLevelUp()
    {
        LevelUpPanel.SetActive(false);
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
        bestText.text = "Best: " + LeaderboardManager.Instance.GetBest();
    }


    public void OnRestartButton()
    {
        GameManager.Instance.Restart();
    }

    public void OnContinueButton()
    {
        GameManager.Instance.ContinueGame();
    }

    //public void OnContinueButton()
    //{
    //    // show ad; on completion if rewarded -> continue
    //    LevelPlayAdsManager.Instance.ShowRewarded(watched =>
    //    {
    //        if (watched)
    //        {
    //            GameManager.Instance.ContinueGame();
    //        }
    //        else
    //        {
    //            // optional: show message "Ad not ready" or re-show continue panel
    //            UIManager.Instance.ShowAdNotAvailablePopup(); // implement if you like
    //        }
    //    });
    //}

    public void UpdatePlanet(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, planets.Length - 1);
        planetText.text = planets[index];
    }

}