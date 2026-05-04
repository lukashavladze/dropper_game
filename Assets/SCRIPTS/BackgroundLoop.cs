using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxFactor = 0.2f;

    private float spriteHeight;
    private float startY;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        spriteHeight = GetComponent<SpriteRenderer>().bounds.size.y;
        Debug.Log("Sprite Height: " + spriteHeight);
        startY = transform.position.y;
    }

    void Update()
    {
        // ✅ Parallax movement
        float targetY = startY + cameraTransform.position.y * parallaxFactor;
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

        // ✅ Looping
        if (cameraTransform.position.y > transform.position.y + spriteHeight)
        {
            startY += spriteHeight * 2f;
        }
    }
}