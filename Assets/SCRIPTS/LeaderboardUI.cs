using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    public Transform content;
    public GameObject itemPrefab;

    public void Show()
    {
        gameObject.SetActive(true);

        // clear old entries
        foreach (Transform child in content)
            Destroy(child.gameObject);

        LeaderboardManager.Instance.LoadTopScores(OnLoaded);
    }

    void OnLoaded(List<LeaderboardManager.LeaderEntry> list)
    {
        foreach (var entry in list)
        {
            GameObject obj = Instantiate(itemPrefab, content);

            var texts = obj.GetComponentsInChildren<TextMeshProUGUI>();

            texts[0].text = entry.name;
            texts[1].text = entry.score.ToString();
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}