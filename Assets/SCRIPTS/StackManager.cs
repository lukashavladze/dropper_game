using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;

public class StackManager : MonoBehaviour
{
    public static StackManager Instance { get; private set; }

    private List<GameObject> stack = new List<GameObject>();

    [Header("Stacking Settings")]
    public float allowedXError = 1.2f;   // X offset tolerance for stacking
    public float allowedYError = 0.4f;   // Y distance tolerance from platform for first stone
    public float overlapRadius = 1.0f;   // radius used to detect platform contact

    private float platformY;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        var platform = GameObject.FindGameObjectWithTag("Platform");
        if (platform)
            platformY = platform.transform.position.y;
    }

    /// <summary>
    /// Called when a stone finishes falling (from FallingObject).
    /// </summary>
    public void RegisterPlacedStone(GameObject stone)
    {
        StartCoroutine(CheckPlacementNextFrame(stone));
    }

    private IEnumerator CheckPlacementNextFrame(GameObject stone)
    {
        yield return null; // Wait one frame for physics to settle

        bool success = false;
        bool isPerfectPlacement = false;

        var rb = stone.GetComponent<Rigidbody2D>();
        if (rb == null)
            yield break;

        // === FIRST STONE (check platform contact) ===
        if (stack.Count == 0)
        {
            Vector2 checkPos = stone.transform.position + Vector3.down * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, overlapRadius);

            foreach (var c in hits)
            {
                if (c.gameObject == stone) continue;
                if (c.CompareTag("Platform"))
                {
                    float yDiff = Mathf.Abs(c.transform.position.y - stone.transform.position.y);
                    if (yDiff < allowedYError + 1f)
                    {
                        success = true;
                        break;
                    }
                }
            }

            if (success)
            {
                var platform = hits.FirstOrDefault(h => h.CompareTag("Platform"));
                if (platform)
                {
                    Vector3 alignedPos = stone.transform.position;
                    alignedPos.y = platform.transform.position.y + platform.transform.localScale.y / 2f + stone.transform.localScale.y / 2f;
                    stone.transform.position = alignedPos;
                }

                stack.Add(stone);
                GameManager.Instance?.OnPlacedSuccessful(stack.Count, stone);
            }
            else
            {
                GameManager.Instance?.OnMiss(stone);
            }
        }
        else
        {
            // === SUBSEQUENT STONES (check alignment with previous stone) ===
            var top = stack[stack.Count - 1];
            float dx = Mathf.Abs(stone.transform.position.x - top.transform.position.x);
            float dy = stone.transform.position.y - top.transform.position.y;

            float cubeWidth = top.transform.localScale.x;
            float cubeHeight = top.transform.localScale.y;

            bool horizontallyAligned = dx < cubeWidth * 0.5f;
            bool verticallyOnTop = dy > 0 && dy < cubeHeight * 1.5f;

            if (horizontallyAligned && verticallyOnTop)
            {
                success = true;

                // === Bonus check: "Perfect Placement" ===
                float perfectThreshold = cubeWidth * 0.1f; // 10% of width considered "perfect"
                if (dx <= perfectThreshold)
                    isPerfectPlacement = true;

                // Snap perfectly on top
                Vector3 alignedPos = stone.transform.position;
                alignedPos.x = top.transform.position.x;
                alignedPos.y = top.transform.position.y + cubeHeight;
                stone.transform.position = alignedPos;

                stack.Add(stone);

                if (isPerfectPlacement)
                {
                    GameManager.Instance?.OnPerfectPlacement(stack.Count, stone);
                    StartCoroutine(PopStoneEffect(stone)); // 💥 POP effect
                }
                else
                {
                    GameManager.Instance?.OnPlacedSuccessful(stack.Count, stone);
                }
            }
            else
            {
                GameManager.Instance?.OnMiss(stone);
            }
        }

        // === Stop physics after placement ===
        if (success && rb.bodyType != RigidbodyType2D.Static)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static; // Prevent further movement
        }
    }


    public IEnumerator CheckMissWhileFalling(GameObject stone)
    {
        // If this is the very first stone, compare with the platform height
        float referenceY = (stack.Count == 0)
        ? platformY // you can set this to your platform’s Y in Start()
            : stack[stack.Count - 1].transform.position.y;

        // Keep checking until game over or stone stops
        while (stone != null)
        {
            if (stone.transform.position.y < referenceY - 1f) // 1f = tolerance below last stone
            {
                Debug.Log("❌ Stone fell below stack — instant game over");
                GameManager.Instance?.OnMiss(stone);
                yield break; // stop checking further
            }

            yield return null;
        }
    }



    public int GetStackCount() => stack.Count;

    /// <summary>
    /// Destroys all stacked stones and clears the list.
    /// </summary>
    public void ResetStack()
    {
        foreach (var s in stack)
        {
            if (s) Destroy(s);
        }
        stack.Clear();
    }


    private IEnumerator PopStoneEffect(GameObject stone)
    {
        if (stone == null) yield break;

        Vector3 originalScale = stone.transform.localScale;
        Vector3 popScale = originalScale * 1.2f; // Slightly larger
        float duration = 0.15f;
        float t = 0f;

        // Scale up
        while (t < duration)
        {
            t += Time.deltaTime;
            stone.transform.localScale = Vector3.Lerp(originalScale, popScale, t / duration);
            yield return null;
        }

        // Scale back down
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            stone.transform.localScale = Vector3.Lerp(popScale, originalScale, t / duration);
            yield return null;
        }

        stone.transform.localScale = originalScale;
    }
}
