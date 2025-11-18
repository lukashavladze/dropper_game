using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Google;
using System;

public class GoogleLogin : MonoBehaviour
{
    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    public void SignInWithGoogle()
    {
        Debug.Log("Google Login BUTTON CLICKED!");
        StartCoroutine(GoogleSignInRoutine());
    }

    private IEnumerator GoogleSignInRoutine()
    {
#if UNITY_ANDROID
    string clientId = "268879827526-4v51plsbmg4qs5e70gir9qs46sp3ivlv.apps.googleusercontent.com";
        
    GoogleSignInConfiguration config = new GoogleSignInConfiguration
    {
        WebClientId = clientId,
        RequestEmail = true,
        RequestIdToken = true
    };

    GoogleSignIn.Configuration = config;

    Task<GoogleSignInUser> signInTask = GoogleSignIn.DefaultInstance.SignIn();

    while (!signInTask.IsCompleted)
        yield return null;

    if (signInTask.IsCanceled || signInTask.IsFaulted)
    {
        Debug.LogError("Google Sign-In Failed");
        yield break;
    }

    GoogleSignInUser googleUser = signInTask.Result;

    Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

    var authTask = auth.SignInWithCredentialAsync(credential);

    while (!authTask.IsCompleted)
        yield return null;

    if (authTask.IsCompletedSuccessfully)
    {
        Debug.Log("Signed in: " + auth.CurrentUser.Email);
    }
#endif

        // <-- This fixes your error!
        yield break;
    }
}