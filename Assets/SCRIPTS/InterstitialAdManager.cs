using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class InterstitialAdManager : MonoBehaviour
{
    public static InterstitialAdManager Instance;

    private InterstitialAd interstitial;

    private string adUnitId = "ca-app-pub-9219903637701882/5648363750";    // real
    //private string adUnitId = "ca-app-pub-9219903637701882/5648363750";  //test

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

    public void LoadAd()
    {
        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }

        AdRequest request = new AdRequest();

        InterstitialAd.Load(adUnitId, request,
        (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
                return;

            interstitial = ad;

            interstitial.OnAdFullScreenContentClosed += () =>
            {
                LoadAd();
            };

            interstitial.OnAdFullScreenContentFailed += (AdError err) =>
            {
                LoadAd();
            };
        });
    }

    public bool IsReady()
    {
        return interstitial != null &&
               interstitial.CanShowAd();
    }

    public void Show(Action onClosed = null)
    {
        if (!IsReady())
        {
            onClosed?.Invoke();
            return;
        }

        interstitial.OnAdFullScreenContentClosed += () =>
        {
            interstitial.Destroy();
            interstitial = null;

            onClosed?.Invoke();

            LoadAd();
        };

        interstitial.OnAdFullScreenContentFailed += (AdError error) =>
        {
            onClosed?.Invoke();
            LoadAd();
        };

        interstitial.Show();
    }
}