using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{


    public void PlayGame()
    {
        // Make sure your main game scene is added in Build Settings
        SceneManager.LoadScene("SampleScene"); // Replace with your main scene name
    }

    public void ExitGame()
    {
        Application.Quit();

        // In Editor, Application.Quit() does nothing, so this is just for testing:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
