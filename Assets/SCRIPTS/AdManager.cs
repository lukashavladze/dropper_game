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
        // TRY ADMOB FIRST
        if (RewardedAdManager.Instance != null &&
            RewardedAdManager.Instance.IsReady())
        {

            RewardedAdManager.Instance.Show(success =>
            {
                if (success)
                {
                    onResult?.Invoke(true);
                }
                else
                {
                    TryUnityAds(onResult);
                }
            });

            return;
        }

        //  FALLBACK
        TryUnityAds(onResult);
    }

    public void ShowInterstitial(Action onClosed = null)
    {
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

    void TryUnityAds(Action<bool> onResult)
    {

        if (UnityAdsManager.Instance != null)
        {
            UnityAdsManager.Instance.ShowRewarded(onResult);
        }
        else
        {
            onResult?.Invoke(false);
        }
    }
}