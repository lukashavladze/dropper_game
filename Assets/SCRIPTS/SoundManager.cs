using UnityEngine;
using TMPro;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("UI")]
    public TextMeshProUGUI soundText;

    private bool soundEnabled = true;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        soundEnabled = PlayerPrefs.GetInt("SOUND_ON", 1) == 1;

        ApplySound();
    }

    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;

        PlayerPrefs.SetInt("SOUND_ON", soundEnabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplySound();
    }

    void ApplySound()
    {
        AudioListener.volume = soundEnabled ? 1f : 0f;

        // UPDATE TEXT
        if (soundText != null)
        {
            soundText.text =
                soundEnabled
                ? "Sounds ON"
                : "Sounds OFF";
        }
    }
}