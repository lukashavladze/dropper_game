using System;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance;

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

    public void ShowRewarded(Action<bool> onResult)
    {
        if (RewardedAdManager.Instance != null &&
            RewardedAdManager.Instance.IsReady())
        {
            RewardedAdManager.Instance.Show(onResult);
        }
        else
        {
            onResult?.Invoke(false);
        }
    }

    public void ShowInterstitial(Action onClosed = null)
    {
        // Never show interstitials to child users
        if (AgeGateManager.Instance != null &&
            AgeGateManager.Instance.IsChild)
        {
            onClosed?.Invoke();
            return;
        }

        if (InterstitialAdManager.Instance != null &&
            InterstitialAdManager.Instance.IsReady())
        {
            InterstitialAdManager.Instance.Show(onClosed);
        }
        else
        {
            onClosed?.Invoke();
        }
    }

    //void TryUnityAds(Action<bool> onResult)
    //{

    //    if (UnityAdsManager.Instance != null)
    //    {
    //        UnityAdsManager.Instance.ShowRewarded(onResult);
    //    }
    //    else
    //    {
    //        onResult?.Invoke(false);
    //    }
    //}
}