using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class BloomController : MonoBehaviour
{
    public static BloomController Instance;

    private Bloom bloom;

    void Awake()
    {
        Instance = this;

        // get Global Volume on this object
        var volume = GetComponent<Volume>();

        if (volume.profile.TryGet(out bloom))
        {
            Debug.Log("✅ Bloom found");
        }
        else
        {
            Debug.LogError("❌ Bloom NOT found in Volume Profile");
        }
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
        SetupBloom();
    }

    void SetupBloom()
    {
        var volume = FindFirstObjectByType<Volume>();

        if (volume != null && volume.profile.TryGet(out bloom))
        {
            Debug.Log("✅ Bloom re-linked");
        }
        else
        {
            Debug.LogWarning("⚠️ No Bloom in this scene");
            bloom = null;
        }
    }


    public void SetTarget(float intensity, float scatter)
    {
        StopAllCoroutines();
        StartCoroutine(AnimateBloom(intensity, scatter));
    }

    IEnumerator AnimateBloom(float targetIntensity, float targetScatter)
    {
        float duration = 0.25f;

        float startI = bloom.intensity.value;
        float startS = bloom.scatter.value;

        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            bloom.intensity.value = Mathf.Lerp(startI, targetIntensity, t);
            bloom.scatter.value = Mathf.Lerp(startS, targetScatter, t);

            yield return null;
        }
    }
}