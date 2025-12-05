using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpPopup : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup canvasGroup;
    public Image backdrop;
    public Image planetImage;
    public TMP_Text planetNameText;
    public TMP_Text titleText;
    public TMP_Text messageText;

    [Header("Particles")]
    public ParticleSystem[] burstParticles;

    [Header("Timing")]
    public float autoHideDelay = 2f;

    private Animator animator;
    private System.Action onComplete;

    void Awake()
    {
        animator = GetComponent<Animator>();
        HideInstant();
    }

    public void Show(string planetName, Sprite planetSprite, System.Action onDone = null)
    {
        onComplete = onDone;

        planetNameText.text = "You have arrived " + planetName;
        planetImage.sprite = planetSprite;

        gameObject.SetActive(true);
        animator.SetTrigger("Show");

        // play circle-burst particles
        foreach (var p in burstParticles)
            p.Play();

        CancelInvoke(nameof(AutoHide));
        Invoke(nameof(AutoHide), autoHideDelay);
    }

    void AutoHide()
    {
        animator.SetTrigger("Hide");
    }

    /// Called by Animation Event at the end of Hide animation
    public void OnHidden()
    {
        HideInstant();
        onComplete?.Invoke();
    }

    void HideInstant()
    {
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
}
