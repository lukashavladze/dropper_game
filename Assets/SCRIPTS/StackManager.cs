using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
        yield return null; // wait 1 frame to let physics settle

        bool success = false;

        // === FIRST STONE (check platform contact) ===
        if (stack.Count == 0)
        {
            //Collider2D[] hits = Physics2D.OverlapCircleAll(stone.transform.position, overlapRadius);
            Vector2 checkPos = stone.transform.position + Vector3.down * 0.5f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, overlapRadius);
            Debug.DrawLine(stone.transform.position, checkPos, Color.red, 1f);

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
                if (GameManager.Instance)
                    GameManager.Instance.OnPlacedSuccessful(stack.Count, stone);
            }
            else
            {
                Debug.Log("❌ Missed platform — game over");
                if (GameManager.Instance)
                    GameManager.Instance.OnMiss(stone);
            }
        }
        else
        {
            // === SUBSEQUENT STONES (check alignment with top stone) ===
            var top = stack[stack.Count - 1];
            float dx = Mathf.Abs(stone.transform.position.x - top.transform.position.x);

            if (dx <= allowedXError)
            {
                Debug.Log("✅ Good placement on stack");
                stack.Add(stone);
                if (GameManager.Instance)
                    GameManager.Instance.OnPlacedSuccessful(stack.Count, stone);
            }
            else
            {
                Debug.Log("❌ Missed stack alignment — game over");
                if (GameManager.Instance)
                    GameManager.Instance.OnMiss(stone);
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
