using UnityEngine;
using UnityEngine.UI;



public class UIManager : MonoBehaviour
{
    public Text scoreText;
    public Text bestText;
    public Text levelText;
    public Button restartButton;
    public GameObject gameOverPanel;
    


    void Start()
    {
        UpdateBest();
        HideGameOver();
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