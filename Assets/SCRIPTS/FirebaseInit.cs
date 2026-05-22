using UnityEngine;
using Firebase;
using Firebase.Database;

public class FirebaseInit : MonoBehaviour
{
    public static FirebaseDatabase DB;
    public static bool IsReady;

    async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status == DependencyStatus.Available)
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;

            // TRY DEFAULT INSTANCE FIRST
            try
            {
                DB = FirebaseDatabase.DefaultInstance;
                // test access
                DatabaseReference testRef = DB.RootReference;

                IsReady = true;
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogError("DefaultInstance FAILED:");
                Debug.LogError(e);
            }

            // FALLBACK TO URL INSTANCE
            try
            {

                DB = FirebaseDatabase.GetInstance(
                    "https://starbloxx-c4908-default-rtdb.europe-west1.firebasedatabase.app/"
                );

                DatabaseReference testRef = DB.RootReference;

                IsReady = true;
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogError("URL instance FAILED:");
                Debug.LogError(e);
            }

            Debug.LogError("Firebase Database completely failed.");
        }
        else
        {
            Debug.LogError("Firebase dependencies failed: " + status);
        }
    }
}