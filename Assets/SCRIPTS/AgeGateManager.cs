using UnityEngine;

public class AgeGateManager : MonoBehaviour
{
    public static AgeGateManager Instance;

    public bool IsChild { get; private set; } = false;
    public bool IsAgeSelected { get; private set; } = false;

    public GameObject agePanel; // drag your panel here

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

    void Start()
    {
        // Check if already selected before
        if (PlayerPrefs.HasKey("age"))
        {
            int age = PlayerPrefs.GetInt("age");
            IsChild = age == 0;
            IsAgeSelected = true;

            StartConsentFlow();
        }
        else
        {
            if (agePanel != null)
                agePanel.SetActive(true); // 🔥 SHOW UI
        }
    }

    public void SelectChild()
    {
        IsChild = true;
        SaveAndContinue();
    }

    public void SelectAdult()
    {
        IsChild = false;
        SaveAndContinue();
    }

    void SaveAndContinue()
    {
        PlayerPrefs.SetInt("age", IsChild ? 0 : 1);
        PlayerPrefs.Save();

        IsAgeSelected = true;

        //gameObject.SetActive(false);
        if (agePanel != null)
            agePanel.SetActive(false); // 🔥 HIDE UI

        StartConsentFlow();
    }

    void StartConsentFlow()
    {
        if (ConsentManager.Instance != null)
        {
            ConsentManager.Instance.RequestConsent();
        }
        else
        {
            Debug.LogError("ConsentManager missing!");
        }
    }
}