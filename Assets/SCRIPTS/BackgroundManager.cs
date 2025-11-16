using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public SpriteRenderer bgRenderer; // background sprite
    public Sprite[] backgrounds; // optional theme sprites
    public Camera mainCamera;
    public float followSpeed = 4f; // how fast it follows camera
    public float yOffset = 0f;     // adjust if needed

    private int currentThemeIndex = 0;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            UpdateTheme(0);
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // Follow camera's vertical movement smoothly
        Vector3 bgPos = bgRenderer.transform.position;
        bgPos.y = Mathf.Lerp(bgPos.y, mainCamera.transform.position.y + yOffset, Time.deltaTime * followSpeed);
        bgRenderer.transform.position = bgPos;
    }

    public void UpdateTheme(int placedCount)
    {
        if (backgrounds == null || backgrounds.Length == 0)
            return;

        int index = Mathf.Min(placedCount / 3, backgrounds.Length - 1);
        if (index != currentThemeIndex)
        {
            currentThemeIndex = index;
            bgRenderer.sprite = backgrounds[currentThemeIndex];
        }
    }
}
