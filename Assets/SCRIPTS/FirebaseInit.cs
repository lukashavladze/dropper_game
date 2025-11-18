using UnityEngine;
using Firebase;
using System.Threading.Tasks;

public class FirebaseInit : MonoBehaviour
{
    async void Start()
    {
        await Init();
    }

    private async Task Init()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status == DependencyStatus.Available)
        {
            Debug.Log("🔥 Firebase initialized!");
        }
        else
        {
            Debug.LogError("❌ Firebase error: " + status);
        }
    }
}
