using Firebase.Database;
using UnityEngine;
using System.Collections;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    private DatabaseReference db;

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
        // wait until Firebase is ready
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

        db.Child("leaderboard").Child(userId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result.Exists)
                {
                    int oldScore = int.Parse(task.Result.Child("score").Value.ToString());

                    if (score > oldScore)
                        WriteScore(userId, name, score);
                }
                else
                {
                    WriteScore(userId, name, score);
                }
            }
        });
    }

    private void WriteScore(string userId, string name, int score)
    {
        db.Child("leaderboard").Child(userId).SetRawJsonValueAsync(
            JsonUtility.ToJson(new LeaderEntry(name, score))
        );

        Debug.Log("🔥 Score saved: " + score);
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