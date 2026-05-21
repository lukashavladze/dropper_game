using UnityEngine;
using TMPro;

public class UsernamePanel : MonoBehaviour
{
    public TMP_InputField inputField;
    public GameObject panel;
    public LeaderboardUI leaderboardUI;

    public void Submit()
    {
        string username = inputField.text.Trim();

        if (string.IsNullOrEmpty(username))
            return;

        // save locally
        PlayerProfile.SetName(username);

        // update Firebase
        LeaderboardManager.Instance.UpdateUsername(username);

        // hide panel
        panel.SetActive(false);

        // 🔥 refresh leaderboard immediately
        //leaderboardUI.RefreshManually();
    }

    public void OpenChangeUsername()
    {
        panel.SetActive(true);

        inputField.text = PlayerProfile.GetName();

        inputField.ActivateInputField();
    }
}