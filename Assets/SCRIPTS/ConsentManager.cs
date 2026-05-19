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
        RequestConsent();
    }

    public void RequestConsent()
    {

        if (_isRequesting) return;
        _isRequesting = true;


        ConsentRequestParameters request = new ConsentRequestParameters();

        ConsentInformation.Update(request, (FormError error) =>
        {
            if (error != null)
            {
                FinishConsent(false);
                return;
            }


            if (ConsentInformation.IsConsentFormAvailable())
            {
                LoadForm();
            }
            else
            {
                EvaluateConsent(); // IMPORTANT
            }
        });
    }

    void LoadForm()
    {

        ConsentForm.Load((ConsentForm form, FormError error) =>
        {
            if (error != null)
            {
                FinishConsent(false);
                return;
            }


            if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
            {

                form.Show((FormError showError) =>
                {
                    if (showError != null)
                    {
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

            CanShowPersonalizedAds = false;
            FinishConsent(false);
            return;
        }

        // ✅ Adult → use consent result
        var status = ConsentInformation.ConsentStatus;

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