using UnityEngine;
using Unity.Services.Core;
using Unity.Services.LevelPlay;

public class RewardedAdsManager : MonoBehaviour
{
    private LevelPlayRewardedAd rewardedAd;

    public string appKey = "24640ba15";
    public string rewardedAdUnitId = "rgpdffxk0xqjb3op";

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
        Debug.LogError("LevelPlay Init Failed: " + error);
    }

    private void OnAdLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded loaded: " + adInfo.AdUnitId);
    }

    private void OnAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError("Rewarded load failed: " + error);
    }

    private void OnAdDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded displayed");
    }

    private void OnAdDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError("Rewarded display failed: " + error);
    }

    private void OnAdClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded closed");
    }

    private void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.Log("USER REWARDED: " + reward.Amount);
    }

    public void ShowRewarded()
    {
        if (rewardedAd != null && rewardedAd.IsAdReady())
        {
            rewardedAd.ShowAd();
        }
        else
        {
            Debug.Log("Rewarded not ready – loading again");
            rewardedAd.LoadAd();
        }
    }
}
