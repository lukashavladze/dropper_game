using UnityEngine;
using Firebase;

public class FirebaseInit : MonoBehaviour
{
    async void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Debug.Log("Firebase start");

        var status = await FirebaseApp.CheckAndFixDependenciesAsync();

        Debug.Log(status);
    }
}