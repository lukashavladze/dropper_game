using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AgeGateManager : MonoBehaviour
{
    public static AgeGateManager Instance;

    public bool IsChild { get; private set; }
    public bool IsAgeSelected { get; private set; }

    [Header("UI")]
    public GameObject agePanel;
    public TMP_Dropdown birthYearDropdown;
    public Button continueButton;

    private const string BirthYearKey = "BirthYear";
    private const int MinimumYear = 1940;

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
        if (PlayerPrefs.HasKey(BirthYearKey))
        {
            int birthYear = PlayerPrefs.GetInt(BirthYearKey);

            IsChild = GetAge(birthYear) < 13;
            IsAgeSelected = true;

            StartConsentFlow();
            return;
        }

        agePanel.SetActive(true);

        PopulateDropdown();

        continueButton.interactable = false;

        birthYearDropdown.onValueChanged.AddListener(OnBirthYearChanged);
    }

    void PopulateDropdown()
    {
        birthYearDropdown.ClearOptions();

        List<string> years = new List<string>();

        years.Add("Select Birth Year");

        int currentYear = DateTime.Now.Year;

        for (int year = currentYear; year >= MinimumYear; year--)
        {
            years.Add(year.ToString());
        }

        birthYearDropdown.AddOptions(years);

        birthYearDropdown.value = 0;
        birthYearDropdown.RefreshShownValue();
    }

    void OnBirthYearChanged(int index)
    {
        // Enable Continue only after a real year is selected
        continueButton.interactable = index != 0;
    }

    public void Continue()
    {
        if (birthYearDropdown.value == 0)
            return;

        int birthYear = int.Parse(
            birthYearDropdown.options[birthYearDropdown.value].text);

        PlayerPrefs.SetInt(BirthYearKey, birthYear);
        PlayerPrefs.Save();

        IsChild = GetAge(birthYear) < 13;
        IsAgeSelected = true;

        agePanel.SetActive(false);

        StartConsentFlow();
    }

    int GetAge(int birthYear)
    {
        int currentYear = DateTime.Now.Year;
        return currentYear - birthYear;
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