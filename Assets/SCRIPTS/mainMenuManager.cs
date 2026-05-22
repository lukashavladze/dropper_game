using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{


    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); 
    }

    public void ExitGame()
    {
        Application.Quit();

        // In Editor this is just for testing:
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
