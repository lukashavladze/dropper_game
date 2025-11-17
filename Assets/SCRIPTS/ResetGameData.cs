using UnityEngine;

public class ResetGameData : MonoBehaviour
{
    void Awake()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("ALL DATA CLEARED");
    }
}
