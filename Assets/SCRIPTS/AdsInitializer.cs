using UnityEngine;
using Unity.Services.Core;
using Unity.Services.LevelPlay;

public class AdsInitializer : MonoBehaviour
{
    public string appKey = "24640ba15";

    async void Start()
    {
        await UnityServices.InitializeAsync();

        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;

        LevelPlay.Init(appKey, SystemInfo.deviceUniqueIdentifier);

    }

    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay Init Success");

        //// Inform RewardedAdsManager AFTER Init
        //FindObjectOfType<RewardedAdsManager>().InitializeRewarded();
    }

    private void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogError("Init Failed: " + error.ToString());
    }
}
