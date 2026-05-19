using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class RewardedAdManager : MonoBehaviour
{
    public static RewardedAdManager Instance;

    private RewardedAd rewardedAd;
    private string adUnitId = "ca-app-pub-3940256099942544/5224354917"; // test

    //private string aDUnitId = "ca-app-pub-9219903637701882/422604285"      // this is real ID



    // ca-app-pub-9219903637701882~1324547345     << this needs to be added in unity
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
    }

    public void LoadAd()
    {
        AdRequest request = new AdRequest();

        RewardedAd.Load(adUnitId, request, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {

                return;
            }

            rewardedAd = ad;

            rewardedAd.OnAdFullScreenContentClosed += () =>
            {

                LoadAd();
            };

            rewardedAd.OnAdFullScreenContentFailed += (err) =>
            {

                LoadAd();
            };

        });
    }

    public bool IsReady()
    {
        return rewardedAd != null;
    }

    public void Show(Action<bool> onResult)
    {
        if (rewardedAd != null)
        {
            rewardedAd.Show((Reward reward) =>
            {
                onResult?.Invoke(true);
            });
        }
        else
        {
            onResult?.Invoke(false);
        }
    }
}