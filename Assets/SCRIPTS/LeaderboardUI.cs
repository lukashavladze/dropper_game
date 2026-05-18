using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LeaderboardUI : MonoBehaviour
{
    public Transform content;
    public GameObject itemPrefab;
    public GameObject usernamePanel;

    void OnEnable()
    {
        if (LeaderboardManager.Instance != null)
            LeaderboardManager.Instance.OnScoreSaved += Refresh;
    }

    void OnDisable()
    {
        if (LeaderboardManager.Instance != null)
            LeaderboardManager.Instance.OnScoreSaved -= Refresh;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (!PlayerProfile.HasName())
        {
            usernamePanel.SetActive(true);
        }
        else
        {
            Load();
        }
    }

    void Refresh()
    {
        Load();
    }

    void Load()
    {
        StartCoroutine(LoadRoutine());
    }

    IEnumerator LoadRoutine()
    {
        // clear old entries
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // wait one frame so objects actually disappear
        yield return null;

        LeaderboardManager.Instance.LoadTopScores(OnLoaded);
    }

    void OnLoaded(List<LeaderboardManager.LeaderEntry> list)
    {
        int rank = 1;

        foreach (var entry in list)
        {
            GameObject obj = Instantiate(itemPrefab, content);

            var item = obj.GetComponent<LeaderboardItemUI>();
            item.Setup(rank, entry.name, entry.score);

            rank++;
        }
    }

    public void RefreshManually()
    {
        Load();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}