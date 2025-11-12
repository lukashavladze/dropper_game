using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class StackManager : MonoBehaviour
{
    public static StackManager Instance { get; private set; }

    private List<GameObject> stack = new List<GameObject>();

    [Header("Stacking Settings")]
    public float allowedXError = 1.2f;   // X offset tolerance for stacking
    public float allowedYError = 0.4f;   // Y distance tolerance from platform for first stone
    public float overlapRadius = 1.0f;   // radius used to detect platform contact

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        yield return null; // wait 1 frame for physics to settle

        bool success = false;

        // === FIRST STONE (check platform contact) ===
        if (stack.Count == 0)
        {
            Vector2 checkPos = stone.transform.position + Vector3.down * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, overlapRadius);

            foreach (var c in hits)
            {
                if (c.gameObject == stone) continue; // ignore self
                if (c.CompareTag("Platform"))
                {
                    float yDiff = Mathf.Abs(c.transform.position.y - stone.transform.position.y);
                    Debug.Log($"Platform hit detected: Δy={yDiff}");

                    if (yDiff < allowedYError + 1f)
                    {
                        success = true;
                        break;
                    }
                }
            }

            if (success)
            {
                Debug.Log("✅ First stone landed correctly");
                stack.Add(stone);

                // Snap stone perfectly onto the platform
                var platform = hits.FirstOrDefault(h => h.CompareTag("Platform"));
                if (platform)
                {
                    Vector3 alignedPos = stone.transform.position;
                    alignedPos.y = platform.transform.position.y + platform.transform.localScale.y / 2f + stone.transform.localScale.y / 2f;
                    stone.transform.position = alignedPos;
                }

                GameManager.Instance?.OnPlacedSuccessful(stack.Count, stone);
            }
            else
            {
                Debug.Log("❌ Missed platform — game over");
                GameManager.Instance?.OnMiss(stone);
            }
        }
        else
        {
            // === SUBSEQUENT STONES (check alignment with top stone) ===
            var top = stack[stack.Count - 1];

            float dx = Mathf.Abs(stone.transform.position.x - top.transform.position.x);
            float dy = stone.transform.position.y - top.transform.position.y;

            float cubeWidth = top.transform.localScale.x;
            float cubeHeight = top.transform.localScale.y;

            bool horizontallyAligned = dx < cubeWidth * 0.5f;
            bool verticallyOnTop = dy > 0 && dy < cubeHeight * 1.5f;

            if (horizontallyAligned && verticallyOnTop)
            {
                Debug.Log("✅ Good placement on stack");
                success = true;

                // Snap perfectly on top of previous stone
                Vector3 alignedPos = stone.transform.position;
                alignedPos.x = top.transform.position.x;
                alignedPos.y = top.transform.position.y + cubeHeight;
                stone.transform.position = alignedPos;

                stack.Add(stone);
                GameManager.Instance?.OnPlacedSuccessful(stack.Count, stone);
            }
            else
            {
                Debug.Log($"❌ Missed stack alignment — dx={dx:F2}, dy={dy:F2}");
                GameManager.Instance?.OnMiss(stone);
            }
        }

        if (success)
        {
            // stop stone physics to keep it fixed in place
            var rb = stone.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
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
}
