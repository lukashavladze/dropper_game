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
        SceneToLoad.nextScene = "menu";
        StartCoroutine(WaitForAgeThenLoad());
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

    IEnumerator WaitForAgeThenLoad()
    {
        // Wait until AgeGateManager exists
        while (AgeGateManager.Instance == null)
            yield return null;

        // 🚨 FIRST TIME → WAIT FOR USER INPUT
        if (!AgeGateManager.Instance.IsAgeSelected)
        {
            Debug.Log("⏳ Waiting for age selection...");

            while (!AgeGateManager.Instance.IsAgeSelected)
                yield return null;
        }

        // 🚨 WAIT FOR CONSENT FLOW TO FINISH
        while (ConsentManager.Instance == null || !ConsentManager.Instance.IsConsentDone)
        {
            yield return null;
        }

        Debug.Log("✅ Age + Consent done → loading game");

        StartCoroutine(LoadAsync());
    }
}
