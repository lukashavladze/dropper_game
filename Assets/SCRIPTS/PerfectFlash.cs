using UnityEngine;

public class PerfectFlash : MonoBehaviour
{
    public float duration = 0.4f;

    private SpriteRenderer sr;
    private float targetWidth;
    private float time;
    private float startY;

    private Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Init(float width)
    {
        targetWidth = width;

        duration *= Random.Range(0.9f, 1.1f);

        transform.localScale = new Vector3(width * 0.4f, 0.12f, 1f);

        startY = transform.position.y;

        baseColor = new Color(0.2f, 1f, 1f, 1f); // cyan
        sr.color = baseColor;

        time = 0f;
    }

    void Update()
    {
        time += Time.deltaTime;
        float t = time / duration;

        // smoother animation
        float curve = t * t * (3f - 2f * t);

        float width = Mathf.Lerp(targetWidth * 0.4f, targetWidth, curve);
        float height = Mathf.Lerp(0.12f, 0.02f, curve);

        transform.localScale = new Vector3(width, height, 1f);

        // vertical motion
        float yOffset = Mathf.Lerp(0.08f, 0f, curve);
        transform.position = new Vector3(transform.position.x, startY + yOffset, transform.position.z);

        // ✅ proper color control (NO accumulation)
        if (sr != null)
        {
            float brightness = Mathf.Lerp(1.4f, 1f, curve);

            Color c = baseColor * brightness;
            c.a = Mathf.Lerp(1f, 0f, curve);

            sr.color = c;
        }

        if (t >= 1f)
            Destroy(gameObject);
    }
}