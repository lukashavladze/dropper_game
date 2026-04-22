using System.Collections;
using TMPro;
using UnityEngine;

public class ScorePopup : MonoBehaviour
{
    public TextMeshPro text;
    public float moveSpeed = 1.5f;
    public float lifetime = 1.5f;

    private Vector3 moveDir;
    private Vector3 targetScale;

    void Start()
    {
        // 🔀 random direction (feels natural)
        float randomX = Random.Range(-0.7f, -0.3f);
        float randomY = Random.Range(0.8f, 1.2f);
        moveDir = new Vector3(randomX, randomY, 0f).normalized;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // movement
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // fade out
        if (text != null)
        {
            Color c = text.color;
            c.a -= Time.deltaTime * 1.5f;
            text.color = c;
        }
    }

    void LateUpdate()
    {
        // always face camera
        if (Camera.main != null)
            transform.forward = Camera.main.transform.forward;
    }

    public void Setup(int amount)
    {
        if (text == null) return;

        text.text = "+" + amount;

        // 🎨 COLOR
        if (amount <= 1)
            text.color = Color.white;
        else if (amount < 20)
            text.color = Color.green;
        else if (amount < 50)
            text.color = new Color(1f, 0.5f, 0f); // orange
        else
            text.color = Color.red;

        // 📏 SCALE
        float scale = Mathf.Clamp(1f + (amount * 0.05f), 1f, 1.8f);
        targetScale = Vector3.one * scale;

        // start from zero for pop
        transform.localScale = Vector3.zero;

        StartCoroutine(Pop());
    }

    IEnumerator Pop()
    {
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 8f;

            // smooth pop
            float curve = Mathf.Sin(t * Mathf.PI * 0.5f);

            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, curve);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}