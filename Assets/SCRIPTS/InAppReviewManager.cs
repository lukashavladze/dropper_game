using System.Collections;
using Google.Play.Review;
using UnityEngine;

public class InAppReviewManager : MonoBehaviour
{
    public static InAppReviewManager Instance;

    private ReviewManager reviewManager;
    private PlayReviewInfo playReviewInfo;

    void Awake()
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
        StartCoroutine(RequestReviewFlow());
    }

    IEnumerator RequestReviewFlow()
    {
        reviewManager = new ReviewManager();

        var requestFlowOperation = reviewManager.RequestReviewFlow();

        yield return requestFlowOperation;

        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            yield break;
        }

        playReviewInfo = requestFlowOperation.GetResult();

        var launchFlowOperation =
            reviewManager.LaunchReviewFlow(playReviewInfo);

        yield return launchFlowOperation;

        if (launchFlowOperation.Error == ReviewErrorCode.NoError)
        {
            // assume user interacted with review dialog
            PlayerPrefs.SetInt("PLAYER_RATED", 1);
            PlayerPrefs.Save();
        }

        playReviewInfo = null;
    }
}