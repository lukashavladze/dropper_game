using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class InterstitialAdManager : MonoBehaviour
{
    public static InterstitialAdManager Instance;

    private InterstitialAd interstitial;

    private string adUnitId = "ca-app-pub-9219903637701882/5648363750";

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

        InterstitialAd.Load(adUnitId, request, (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.Log("Interstitial failed to load");
                return;
            }

            Debug.Log("Interstitial loaded");

            interstitial = ad;
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

        InterstitialAd ad = interstitial;
        interstitial = null;

        bool handled = false;

        void Finish()
        {
            if (handled)
                return;

            handled = true;

            ad.Destroy();
            onClosed?.Invoke();
            LoadAd();
        }

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("Interstitial closed");
            Finish();
        };

        ad.OnAdFullScreenContentFailed += (error) =>
        {
            Debug.Log($"Interstitial failed to show: {error}");
            Finish();
        };

        ad.Show();
    }
}