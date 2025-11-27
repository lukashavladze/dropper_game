//#if UNITY_EDITOR
//using System;
//using UnityEngine;

//public class EditorRewardedAdSimulator
//{
//    public event Action OnAdLoaded;
//    public event Action OnAdDisplayed;
//    public event Action OnAdClosed;
//    public event Action OnAdRewarded;

//    public void LoadAd()
//    {
//        Debug.Log("<Editor Ad> Simulating load...");
//        OnAdLoaded?.Invoke();
//    }

//    public bool IsAdReady()
//    {
//        return true; // Always ready in editor
//    }

//    public void ShowAd()
//    {
//        Debug.Log("<Editor Ad> Simulated ad started");
//        OnAdDisplayed?.Invoke();

//        // Simulate user reward
//        Debug.Log("<Editor Ad> Simulating reward");
//        OnAdRewarded?.Invoke();

//        Debug.Log("<Editor Ad> Simulated ad closed");
//        OnAdClosed?.Invoke();
//    }
//}
//#endif
