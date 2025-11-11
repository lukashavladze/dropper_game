using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FallingObject : MonoBehaviour
{
    public float sleepVelocityThreshold = 0.05f;
    public float settleTime = 0.25f; // time under threshold to be considered settled
    private float _timer;
    private Rigidbody2D rb;
    private bool placed = false;


    public event Action<GameObject> OnPlaced;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.simulated = false; // start off non-simulated until dropped
    }


    void FixedUpdate()
    {
        if (placed) return;
        if (rb.simulated)
        {
            if (rb.linearVelocity.sqrMagnitude < sleepVelocityThreshold * sleepVelocityThreshold)
            {
                _timer += Time.fixedDeltaTime;
                if (_timer >= settleTime)
                {
                    placed = true;
                    rb.bodyType = RigidbodyType2D.Static; // freeze in place
                    OnPlaced?.Invoke(gameObject);
                }
            }
            else
            {
                _timer = 0f;
            }
        }
    }
}