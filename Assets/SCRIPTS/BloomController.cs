
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class BloomController : MonoBehaviour
{
    public static BloomController Instance;

    private Bloom bloom;
    Coroutine pulseRoutine;
    Coroutine colorRoutine;
    void Awake()
    {
        Instance = this;

        // get Global Volume on this object
        var volume = GetComponent<Volume>();
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

    public void Pulse(float peakI, float peakS)
    {
        if (bloom == null) return;

        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(PulseRoutine(peakI, peakS));
    }

    IEnumerator PulseRoutine(float peakI, float peakS)
    {
        float upTime = 0.08f;     // fast hit
        float downTime = 0.25f;   // smooth decay

        float baseI = bloom.intensity.value;
        float baseS = bloom.scatter.value;

        float t = 0;

        // 🔥 GO UP FAST
        while (t < 1f)
        {
            t += Time.deltaTime / upTime;
            bloom.intensity.value = Mathf.Lerp(baseI, peakI, t);
            bloom.scatter.value = Mathf.Lerp(baseS, peakS, t);
            yield return null;
        }

        t = 0;

        // 🌫 GO DOWN SMOOTH
        while (t < 1f)
        {
            t += Time.deltaTime / downTime;
            bloom.intensity.value = Mathf.Lerp(peakI, baseI, t);
            bloom.scatter.value = Mathf.Lerp(peakS, baseS, t);
            yield return null;
        }
    }

    public void SetColor(Color targetColor)
    {
        if (bloom == null) return;
        bloom.tint.overrideState = true;
        bloom.tint.value = targetColor;
    }

    public void AnimateColor(Color target)
    {
        if (bloom == null) return;

        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        colorRoutine = StartCoroutine(ColorRoutine(target));
    }

    IEnumerator ColorRoutine(Color target)
    {
        if (bloom == null) yield break;

        bloom.tint.overrideState = true; // 🔥 IMPORTANT

        float duration = 0.3f;
        float t = 0;

        Color start = bloom.tint.value;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            bloom.tint.value = Color.Lerp(start, target, t);
            yield return null;
        }
    }
}