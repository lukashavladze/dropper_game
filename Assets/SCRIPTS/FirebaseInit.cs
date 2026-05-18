using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;

public class FirebaseInit : MonoBehaviour
{
    public static FirebaseDatabase DB;
    public static bool IsReady = false;

    async void Awake()
    {
        DontDestroyOnLoad(gameObject);

        var status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status == DependencyStatus.Available)
        {
            DB = FirebaseDatabase.GetInstance(
                "https://starbloxx-c4908-default-rtdb.europe-west1.firebasedatabase.app/"
            );

            IsReady = true;
        }
    }
}