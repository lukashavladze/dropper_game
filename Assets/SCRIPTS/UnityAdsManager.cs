using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class UnityAdsManager : MonoBehaviour,
    IUnityAdsShowListener,
    IUnityAdsLoadListener,
    IUnityAdsInitializationListener
{
    public static UnityAdsManager Instance;

    [SerializeField] string androidGameId = "5992767";
    [SerializeField] string iosGameId = "5992766";

    [SerializeField] string androidRewardedId = "oncontinue";
    [SerializeField] string iosRewardedId = "oncontinueios"; // make sure this exists

    private string rewardedAdUnitId; // 🔥 active ID based on platform
    private Action<bool> _onAdResult;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID
            rewardedAdUnitId = androidRewardedId;
            Advertisement.Initialize(androidGameId, true, this);
#elif UNITY_IOS
            rewardedAdUnitId = iosRewardedId;
            Advertisement.Initialize(iosGameId, true, this);
#endif
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Ads Initialized");
        Debug.Log("Using Placement: " + rewardedAdUnitId);
        LoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Init failed: {message}");
    }

    public void LoadAd()
    {
        Advertisement.Load(rewardedAdUnitId, this);
    }

    public void ShowRewarded(Action<bool> onResult)
    {
        _onAdResult = onResult;

        if (Advertisement.isInitialized)
        {
            Advertisement.Show(rewardedAdUnitId, this);
        }
        else
        {
            Debug.Log("Ads not ready");
            _onAdResult?.Invoke(false);
        }
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Ad Loaded: " + placementId);
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Failed to load ad: {message}");
    }

    public void OnUnityAdsShowStart(string placementId) { }

    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Failed to show ad: {message}");
        _onAdResult?.Invoke(false);
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState state)
    {
        if (placementId == rewardedAdUnitId)
        {
            bool success = state == UnityAdsShowCompletionState.COMPLETED;
            _onAdResult?.Invoke(success);
            LoadAd(); // preload next ad
        }
    }
}