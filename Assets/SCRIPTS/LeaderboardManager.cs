using UnityEngine;


public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }
    private const string BEST_KEY = "BestScore";


    void Awake() { Instance = this; }


    public int GetBest()
    {
        return PlayerPrefs.GetInt(BEST_KEY, 0);
    }


    public void SetBest(int val)
    {
        PlayerPrefs.SetInt(BEST_KEY, val);
        PlayerPrefs.Save();
    }
}