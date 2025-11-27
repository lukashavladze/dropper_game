using UnityEngine;
using Unity.Services.Core;
using Unity.Services.LevelPlay;
using Unity.VisualScripting;

public class RewardedAdsManager : MonoBehaviour
{
    private LevelPlayRewardedAd rewardedAd;

    public string appKey = "24640ba15";
    public string rewardedAdUnitId = "rgpdffxk0xqjb3op";

    async void Start()
    {
        await UnityServices.InitializeAsync();

        // Subscribe with correct signatures
        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;

        LevelPlay.Init(appKey, SystemInfo.deviceUniqueIdentifier);
    }

    // -------- INIT CALLBACKS --------

    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay Init Success");

#if UNITY_EDITOR
        Debug.Log("Editor mode: LevelPlay ads cannot load in editor.");
        return;
#endif

        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);

        rewardedAd.OnAdLoaded += OnAdLoaded;
        rewardedAd.OnAdLoadFailed += OnAdLoadFailed;
        rewardedAd.OnAdDisplayed += OnAdDisplayed;
        rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
        rewardedAd.OnAdClosed += OnAdClosed;
        rewardedAd.OnAdRewarded += OnAdRewarded;

        rewardedAd.LoadAd();
    }


    private void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogError("Init Failed:");
    }

    // -------- REWARDED EVENTS --------

    private void OnAdLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded loaded: " + adInfo.AdUnitId);
    }

    private void OnAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError("Load failed: ");
    }

    private void OnAdDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded displayed");
    }

    private void OnAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError("Display failed:");
    }

    private void OnAdClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded closed");
    }

    private void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log("User rewarded: " + reward.Amount);
        // Give reward here
    }

    public void ShowRewarded()
    {
        if (rewardedAd != null && rewardedAd.IsAdReady())
            rewardedAd.ShowAd();
        else
        {
            Debug.Log("Rewarded not ready — reloading");
            rewardedAd.LoadAd();
        }
    }
}
