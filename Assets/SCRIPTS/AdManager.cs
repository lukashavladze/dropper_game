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
        // 🔥 TRY ADMOB FIRST
        if (RewardedAdManager.Instance != null &&
            RewardedAdManager.Instance.IsReady())
        {
            Debug.Log("➡️ Showing AdMob");

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

        // 🔥 FALLBACK
        TryUnityAds(onResult);
    }

    void TryUnityAds(Action<bool> onResult)
    {
        Debug.Log("➡️ Falling back to Unity Ads");

        if (UnityAdsManager.Instance != null)
        {
            UnityAdsManager.Instance.ShowRewarded(onResult);
        }
        else
        {
            Debug.Log("❌ No ads available");
            onResult?.Invoke(false);
        }
    }
}