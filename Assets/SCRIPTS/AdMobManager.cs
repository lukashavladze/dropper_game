using GoogleMobileAds.Api;
using UnityEngine;

public class AdMobManager : MonoBehaviour
{
    public static AdMobManager Instance;

    public bool IsInitialized { get; private set; } = false;

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

    //  CALLED AFTER CONSENT
    public void InitializeAds(RequestConfiguration config)
    {
        if (IsInitialized)
            return;

        MobileAds.SetRequestConfiguration(config);

        MobileAds.Initialize(initStatus =>
        {
            IsInitialized = true;

            // Load first ad
            RewardedAdManager.Instance?.LoadAd();
            if (AgeGateManager.Instance == null || !AgeGateManager.Instance.IsChild)
            {
                InterstitialAdManager.Instance?.LoadAd();
            }
        });
    }
}