using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        FitToCamera();
    }

    void FitToCamera()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;

        float worldHeight = Camera.main.orthographicSize * 1f;
        float worldWidth = worldHeight * Camera.main.aspect;

        Vector3 scale = transform.localScale;
        scale.x = worldWidth / spriteWidth;
        scale.y = worldHeight / spriteHeight;

        transform.localScale = scale;
    }
}
