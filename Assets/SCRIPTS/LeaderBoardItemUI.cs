using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardItemUI : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    public Image background;
    public Image rankCircle;

    public void Setup(int rank, string name, int score)
    {
        rankText.text = rank.ToString();
        nameText.text = name;
        scoreText.text = score.ToString();

        if (rank == 1)
        {
            background.color = new Color(1f, 0.75f, 0.1f); // gold
            rankCircle.color = new Color(1f, 0.9f, 0.2f);

            nameText.color = Color.black;
            scoreText.color = Color.black;
        }
        else if (rank == 2)
        {
            background.color = new Color(0.7f, 0.7f, 0.7f); // silver
            rankCircle.color = Color.white;

            nameText.color = Color.black;
            scoreText.color = Color.black;
        }
        else if (rank == 3)
        {
            background.color = new Color(0.8f, 0.4f, 0.1f); // bronze
            rankCircle.color = new Color(1f, 0.6f, 0.2f);

            nameText.color = Color.black;
            scoreText.color = Color.black;
        }
        else
        {
            background.color = new Color(0f, 0.1f, 0.2f);
            rankCircle.color = new Color(0f, 0.4f, 0.6f);

            nameText.color = Color.white;
            scoreText.color = Color.cyan;
        }
    }
}