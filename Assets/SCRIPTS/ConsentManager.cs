using UnityEngine;
using GoogleMobileAds.Ump.Api;
using System;

public class ConsentManager : MonoBehaviour
{
    public static ConsentManager Instance;

    public bool CanShowPersonalizedAds { get; private set; } = false;
    public bool IsConsentDone { get; private set; } = false;

    private bool _isRequesting = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 🔥 ADD
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
       // RequestConsent();
    }

    public void RequestConsent()
    {

        if (_isRequesting) return;
        _isRequesting = true;

        Debug.Log("📡 Requesting consent...");

        // 🔥 OPTIONAL: DEBUG SETTINGS (remove in production)   ------------------------------------------------ REMOVE BEFORE PUBLISH----------------------------
        var debugSettings = new ConsentDebugSettings
        {
            DebugGeography = DebugGeography.EEA // simulate EU user
        };

        ConsentRequestParameters request = new ConsentRequestParameters
        {
            ConsentDebugSettings = debugSettings
        };

        ConsentInformation.Update(request, (FormError error) =>
        {
            if (error != null)
            {
                Debug.LogWarning("❌ Consent update error: " + error.Message);
                FinishConsent(false);
                return;
            }

            Debug.Log("✅ Consent info updated");

            if (ConsentInformation.IsConsentFormAvailable())
            {
                LoadForm();
            }
            else
            {
                Debug.Log("ℹ️ No consent form available");
                EvaluateConsent(); // IMPORTANT
            }
        });
    }

    void LoadForm()
    {
        Debug.Log("📥 Loading consent form...");

        ConsentForm.Load((ConsentForm form, FormError error) =>
        {
            if (error != null)
            {
                Debug.LogWarning("❌ Form load error: " + error.Message);
                FinishConsent(false);
                return;
            }

            Debug.Log("✅ Consent form loaded");

            if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
            {
                Debug.Log("📢 Showing consent form...");

                form.Show((FormError showError) =>
                {
                    if (showError != null)
                    {
                        Debug.LogWarning("❌ Form show error: " + showError.Message);
                        FinishConsent(false);
                        return;
                    }

                    EvaluateConsent();
                });
            }
            else
            {
                // Already consented or not required
                EvaluateConsent();
            }
        });
    }

    void EvaluateConsent()
    {
        bool isChild = AgeGateManager.Instance != null && AgeGateManager.Instance.IsChild;

        if (isChild)
        {
            Debug.Log("👶 Under 13 → forcing NON-personalized ads");

            CanShowPersonalizedAds = false;
            FinishConsent(false);
            return;
        }

        // ✅ Adult → use consent result
        var status = ConsentInformation.ConsentStatus;

        Debug.Log("🔍 Consent Status: " + status);

        switch (status)
        {
            case ConsentStatus.Obtained:
            case ConsentStatus.NotRequired:
                CanShowPersonalizedAds = true;
                break;

            default:
                CanShowPersonalizedAds = false;
                break;
        }

        FinishConsent(CanShowPersonalizedAds);
    }

    void FinishConsent(bool personalized)
    {
        if (IsConsentDone) return;

        IsConsentDone = true;

        Debug.Log("🎯 Consent finished. Personalized: " + personalized);

        // 🔥 Initialize ads AFTER consent
        if (AdInitializer.Instance != null)
        {
            AdInitializer.Instance.InitializeAds(personalized);
        }
        else
        {
            Debug.LogError("❌ AdInitializer not found!");
        }
    }
}