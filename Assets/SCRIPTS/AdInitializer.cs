using UnityEngine;
using GoogleMobileAds.Api;

public class AdInitializer : MonoBehaviour
{
    public static AdInitializer Instance;

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

    public void InitializeAds(bool personalized)
    {
        bool isChild = false;

        if (AgeGateManager.Instance != null)
        {
            isChild = AgeGateManager.Instance.IsChild;
        }

        RequestConfiguration config;

        // 👶 CHILD (UNDER 13)
        if (isChild)
        {

            config = new RequestConfiguration
            {
                TagForChildDirectedTreatment = TagForChildDirectedTreatment.True,
                TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.True,
                MaxAdContentRating = MaxAdContentRating.G
            };
        }
        // 🧑 ADULT + PERSONALIZED
        else if (personalized)
        {
            config = new RequestConfiguration
            {
                TagForChildDirectedTreatment = TagForChildDirectedTreatment.False,
                TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.False,
                MaxAdContentRating = MaxAdContentRating.T
            };
        }
        // 🛡 ADULT + NON-PERSONALIZED
        else
        {
            config = new RequestConfiguration
            {
                TagForChildDirectedTreatment = TagForChildDirectedTreatment.False, // ❗ important
                TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.True,
                MaxAdContentRating = MaxAdContentRating.T
            };
        }

        if (AdMobManager.Instance != null)
        {
            AdMobManager.Instance.InitializeAds(config);
        }
        else
        {
            Debug.LogError("❌ AdMobManager not found in scene!");
        }
    }
}