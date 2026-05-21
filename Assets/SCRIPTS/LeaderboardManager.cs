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

        Debug.Log("Leaderboard DB READY");
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
            return;
        }

        if (score <= 0)
        {
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
                return;
            }

            if (!task.IsCompleted)
            {
                return;
            }

            if (task.Result.Exists)
            {
                int oldScore = 0;

                if (task.Result.Child("score").Exists)
                {
                    int.TryParse(
                        task.Result.Child("score").Value.ToString(),
                        out oldScore
                    );
                }

                if (score > oldScore)
                {
                    WriteScore(userId, name, score);
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
            return;
        }

        string userId = GetUserId();

        db.Child("leaderboard")
          .Child(userId)
          .Child("name")
          .SetValueAsync(newName)
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompleted)
              {
                  OnScoreSaved?.Invoke();
              }
          });
    }

    public void ClearLeaderboardForTesting()
    {
        if (db == null)
        {
            return;
        }

        db.Child("leaderboard").RemoveValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                // refresh UI if open
                OnScoreSaved?.Invoke();
            }
        });
    }

    public void LoadTopScores(System.Action<List<LeaderEntry>> callback)
    {
        if (db == null)
        {
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
                  return;
              }

              if (!task.IsCompleted)
              {
                  return;
              }

              if (task.Result == null || !task.Result.Exists)
              {
                  callback?.Invoke(new List<LeaderEntry>());
                  return;
              }


              var list = new List<LeaderEntry>();

              foreach (var child in task.Result.Children)
              {
                  string name = child.Child("name").Value?.ToString() ?? "Unknown";
                  int score = 0;

                  if (child.Child("score").Exists)
                  {
                      int.TryParse(
                          child.Child("score").Value.ToString(),
                          out score
                      );
                  }

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

                // 🔥 Notify UI AFTER save completes
                OnScoreSaved?.Invoke();
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