using UnityEngine;

#if UNITY_ANDROID
using System.Collections;
using Google.Play.Review;
#endif

#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class InAppReviewManager : MonoBehaviour
{
    public static InAppReviewManager Instance;

#if UNITY_ANDROID
    private ReviewManager reviewManager;
    private PlayReviewInfo playReviewInfo;
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RequestReview()
    {
#if UNITY_ANDROID
        StartCoroutine(RequestAndroidReviewFlow());
#elif UNITY_IOS
        Device.RequestStoreReview();
#else
        Debug.Log("In-app review is unavailable on this platform.");
#endif
    }

#if UNITY_ANDROID
    private IEnumerator RequestAndroidReviewFlow()
    {
        reviewManager = new ReviewManager();

        var requestOperation =
            reviewManager.RequestReviewFlow();

        yield return requestOperation;

        if (requestOperation.Error != ReviewErrorCode.NoError)
        {
            yield break;
        }

        playReviewInfo = requestOperation.GetResult();

        var launchOperation =
            reviewManager.LaunchReviewFlow(playReviewInfo);

        yield return launchOperation;

        playReviewInfo = null;
    }
#endif
}