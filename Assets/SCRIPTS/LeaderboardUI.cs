using UnityEngine;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    public Transform content;
    public GameObject itemPrefab;

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
        Load();
    }

    void Refresh()
    {
        Load();
    }

    void Load()
    {
        // clear old entries
        foreach (Transform child in content)
            Destroy(child.gameObject);

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

    public void Close()
    {
        gameObject.SetActive(false);
    }
}