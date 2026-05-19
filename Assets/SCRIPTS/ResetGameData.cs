//using UnityEngine;
//using Firebase.Database;

//public class ResetGameData : MonoBehaviour
//{
//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            ResetEverything();
//        }
//    }

//    void ResetEverything()
//    {
//        // DELETE ENTIRE LEADERBOARD
//        FirebaseDatabase.DefaultInstance
//            .GetReference("leaderboard")
//            .RemoveValueAsync();

//        // CLEAR PLAYER PREFS
//        PlayerPrefs.DeleteAll();
//        PlayerPrefs.Save();

//        Debug.Log("ALL FIREBASE LEADERBOARD DATA + PLAYERPREFS CLEARED");
//    }
//}