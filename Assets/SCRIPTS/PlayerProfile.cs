using UnityEngine;

public static class PlayerProfile
{
    private const string NAME_KEY = "PLAYER_NAME";

    public static void SetName(string name)
    {
        PlayerPrefs.SetString(NAME_KEY, name);
        PlayerPrefs.Save();
    }

    public static string GetName()
    {
        return PlayerPrefs.GetString(NAME_KEY, "");
    }

    public static bool HasName()
    {
        return PlayerPrefs.HasKey(NAME_KEY) && !string.IsNullOrEmpty(GetName());
    }
}