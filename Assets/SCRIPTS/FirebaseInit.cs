using UnityEngine;
using Firebase;
using Firebase.Database;

public class FirebaseInit : MonoBehaviour
{
    public static FirebaseDatabase DB;
    public static bool IsReady = false;

    async void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Debug.Log("Firebase init started");

        try
        {
            var status = await FirebaseApp.CheckAndFixDependenciesAsync();

            Debug.Log("Firebase dependency status: " + status);

            if (status == DependencyStatus.Available)
            {
                DB = FirebaseDatabase.GetInstance(
                    "https://starbloxx-c4908-default-rtdb.europe-west1.firebasedatabase.app/"
                );

                Debug.Log("Firebase database initialized");

                IsReady = true;
            }
            else
            {
                Debug.LogError("Firebase dependencies failed: " + status);

                // Prevent infinite loading freeze
                IsReady = true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Firebase exception: " + e);

            // Prevent infinite loading freeze
            IsReady = true;
        }
    }
}