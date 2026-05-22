using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private CameraFollow follow;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //  re-find CameraFollow in new scene
        follow = FindFirstObjectByType<CameraFollow>();
    }

    public static void ShakeSafe(float duration, float magnitude)
    {
        if (Instance == null) return;

        Instance.Shake(duration, magnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        if (follow == null)
        {
            follow = FindFirstObjectByType<CameraFollow>();
            if (follow == null)
            {
                return;
            }
        }

        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            follow.shakeOffset = new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        //  HARD RESET (important)
        follow.shakeOffset = Vector3.zero;
    }
}