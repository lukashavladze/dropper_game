using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public Slider progressBar;
    public Text progressText;

    void Start()
    {
        // Always load Menu at game start
        SceneToLoad.nextScene = "menu";
        StartCoroutine(LoadAsync());
    }

    IEnumerator LoadAsync()
    {
        // Allow 1 frame so UI can appear
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(SceneToLoad.nextScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            if (op.progress >= 0.9f)
            {
                // Optional: fake delay to show animation
                yield return new WaitForSeconds(0.5f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
