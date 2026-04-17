using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class UnityAdsManager : MonoBehaviour, IUnityAdsShowListener, IUnityAdsLoadListener, IUnityAdsInitializationListener
{
    public static UnityAdsManager Instance;

    [SerializeField] string androidGameId = "5992767";
    [SerializeField] string iosGameId = "YOUR_IOS_ID";

    [SerializeField] string rewardedAdUnitId = "oncontinue"; // android

    private Action<bool> _onAdResult;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID
            Advertisement.Initialize(androidGameId, true, this);
#elif UNITY_IOS
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

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId == rewardedAdUnitId)
        {
            bool success = showCompletionState == UnityAdsShowCompletionState.COMPLETED;
            _onAdResult?.Invoke(success);
            LoadAd();
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Failed to load ad: {message}");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Failed to show ad: {message}");
        _onAdResult?.Invoke(false);
    }

    public void OnUnityAdsAdLoaded(string placementId) { }

    public void OnUnityAdsShowStart(string placementId) { }

    public void OnUnityAdsShowClick(string placementId) { }
}