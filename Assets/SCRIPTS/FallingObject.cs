using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FallingObject : MonoBehaviour
{
    public float sleepVelocityThreshold = 0.05f;
    public float settleTime = 0.2f;
    private float _timer;
    private Rigidbody2D rb;
    private bool placed = false;

    public event Action<GameObject> OnPlaced;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false;
    }

    void FixedUpdate()
    {
        if (placed) return;
        if (!rb.simulated) return;

        if (rb.linearVelocity.sqrMagnitude < sleepVelocityThreshold * sleepVelocityThreshold)
        {
            _timer += Time.fixedDeltaTime;
            if (_timer >= settleTime)
            {
                placed = true;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Static;
                OnPlaced?.Invoke(gameObject);
            }
        }
        else
        {
            _timer = 0f;
        }
    }

    public void ForcePlace()
    {
        if (placed) return;
        placed = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;
        OnPlaced?.Invoke(gameObject);
    }
}
