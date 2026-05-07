using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    public GameObject settingsPanel;

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL("https://lukashavladze.github.io/STAR-BLOXX/");
    }
}