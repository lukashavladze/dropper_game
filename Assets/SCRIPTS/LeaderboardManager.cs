using Firebase.Database;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    private DatabaseReference db;

    public System.Action OnScoreSaved; // UI refresh hook

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        while (!FirebaseInit.IsReady)
            yield return null;

        db = FirebaseInit.DB.RootReference;

        Debug.Log("🏆 Leaderboard ready");
    }

    public void SaveScore(int score)
    {
        if (db == null)
        {
            Debug.LogWarning("Database not ready yet!");
            return;
        }

        string userId = SystemInfo.deviceUniqueIdentifier;
        string name = "Player_" + Random.Range(1000, 9999);

        db.Child("leaderboard").Child(userId).GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted || task.Result == null)
                return;

            if (task.Result.Exists)
            {
                int oldScore = int.Parse(task.Result.Child("score").Value.ToString());

                if (score > oldScore)
                {
                    WriteScore(userId, name, score);
                }
                else
                {
                    Debug.Log("Score not higher, not updating");
                }
            }
            else
            {
                WriteScore(userId, name, score);
            }
        });
    }

    public void LoadTopScores(System.Action<List<LeaderEntry>> callback)
    {
        db.Child("leaderboard")
          .OrderByChild("score")
          .LimitToLast(10)
          .GetValueAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (!task.IsCompleted || task.Result == null)
                  return;

              var list = new List<LeaderEntry>();

              foreach (var child in task.Result.Children)
              {
                  string name = child.Child("name").Value.ToString();
                  int score = int.Parse(child.Child("score").Value.ToString());

                  list.Add(new LeaderEntry(name, score));
              }

              // Firebase returns ascending → reverse
              list.Sort((a, b) => b.score.CompareTo(a.score));

              callback?.Invoke(list);
          });
    }

    private void WriteScore(string userId, string name, int score)
    {
        db.Child("leaderboard").Child(userId)
        .SetRawJsonValueAsync(JsonUtility.ToJson(new LeaderEntry(name, score)))
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("🔥 Score saved: " + score);

                // 🔥 Notify UI AFTER save completes
                OnScoreSaved?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to save score");
            }
        });
    }

    [System.Serializable]
    public class LeaderEntry
    {
        public string name;
        public int score;

        public LeaderEntry(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }
}