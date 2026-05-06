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

    private string GetUserId()
    {
        if (PlayerPrefs.HasKey("USER_ID"))
            return PlayerPrefs.GetString("USER_ID");

        string id = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString("USER_ID", id);
        PlayerPrefs.Save();

        return id;
    }

    public void SaveScore(int score)
    {
        if (db == null)
        {
            Debug.LogWarning("Database not ready yet!");
            return;
        }

        string userId = GetUserId();
        string name = PlayerProfile.GetName();

        if (string.IsNullOrEmpty(name))
        {
            name = "Player" + Random.Range(100, 9999); // temp name
        }

        db.Child("leaderboard").Child(userId).GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("❌ Read failed: " + task.Exception);
                return;
            }

            if (!task.IsCompleted)
            {
                Debug.LogWarning("⚠️ Read not completed");
                return;
            }

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

    public void UpdateUsername(string newName)
    {
        if (db == null)
        {
            Debug.LogWarning("DB not ready");
            return;
        }

        string userId = GetUserId();

        db.Child("leaderboard").Child(userId).GetValueAsync()
        .ContinueWithOnMainThread(task =>
        {
            if (task.Result.Exists)
            {
                // update only name
                db.Child("leaderboard").Child(userId).Child("name")
                .SetValueAsync(newName);
            }
            else
            {
                // 🔥 create entry if missing
                WriteScore(userId, newName, 0);
            }

            OnScoreSaved?.Invoke();
        });
    }

    public void ClearLeaderboardForTesting()
    {
        if (db == null)
        {
            Debug.LogWarning("DB not ready");
            return;
        }

        db.Child("leaderboard").RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("🔥 Leaderboard CLEARED");

                // refresh UI if open
                OnScoreSaved?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to clear leaderboard");
            }
        });
    }

    public void LoadTopScores(System.Action<List<LeaderEntry>> callback)
    {
        if (db == null)
        {
            Debug.LogError("❌ DB is NULL when loading scores!");
            return;
        }

        db.Child("leaderboard")
          .OrderByChild("score")
          .LimitToLast(10)
          .GetValueAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted)
              {
                  Debug.LogError("❌ Load failed: " + task.Exception);
                  return;
              }

              if (!task.IsCompleted)
              {
                  Debug.LogWarning("⚠️ Load not completed");
                  return;
              }

              if (task.Result == null || !task.Result.Exists)
              {
                  Debug.LogWarning("⚠️ No leaderboard data found");
                  callback?.Invoke(new List<LeaderEntry>());
                  return;
              }

              Debug.Log("✅ Firebase data received!");

              var list = new List<LeaderEntry>();

              foreach (var child in task.Result.Children)
              {
                  Debug.Log("👉 Raw entry: " + child.GetRawJsonValue());

                  string name = child.Child("name").Value?.ToString() ?? "Unknown";
                  int score = int.Parse(child.Child("score").Value.ToString());

                  list.Add(new LeaderEntry(name, score));
              }

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