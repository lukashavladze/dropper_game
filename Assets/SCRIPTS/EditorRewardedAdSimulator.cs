#if UNITY_EDITOR
using System;
using UnityEngine;

public class EditorRewardedAdSimulator
{
    public event Action OnAdLoaded;
    public event Action OnAdDisplayed;
    public event Action OnAdClosed;
    public event Action OnAdRewarded;

    public void LoadAd()
    {
        Debug.Log("<Editor Ad> Simulating load...");
        OnAdLoaded?.Invoke();
    }

    public bool IsAdReady()
    {
        return true;
    }

    public void ShowAd()
    {
        Debug.Log("<Editor Ad> Simulated ad started");

        OnAdDisplayed?.Invoke();

        // Simulate user watching the ad
        Debug.Log("<Editor Ad> Simulating reward");
        OnAdRewarded?.Invoke();

        OnAdClosed?.Invoke();
    }
}
#endif
