using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StackManager : MonoBehaviour
{
    public static StackManager Instance { get; private set; }

    private List<GameObject> stack = new List<GameObject>();

    [Header("Stone Width Rules")]
    public float originalStoneWidth = 1.0f;
    public float minStoneWidth = 0.1f;
    public float perfectThreshold = 0.02f;

    public float fixedShrink = 0.1f;
    public float fixedWiden = 0.1f;

    [Header("Placement Check")]
    public float overlapRadius = 0.2f;
    public float verticalTolerance = 0.5f;

    public float NextStoneWidth { get; private set; }

    private float platformY = 0f;

    void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        NextStoneWidth = originalStoneWidth;
    }

    void Start()
    {
        var platform = GameObject.FindGameObjectWithTag("Platform");
        if (platform)
            platformY = platform.transform.position.y;
    }

    public void RegisterPlacedStone(GameObject stone)
    {
        StartCoroutine(CheckPlacementNextFrame(stone));
    }

    private IEnumerator CheckPlacementNextFrame(GameObject stone)
    {
        yield return null;
        if (stone == null) yield break;

        var rb = stone.GetComponent<Rigidbody2D>();
        var sr = stone.GetComponent<SpriteRenderer>();
        if (!rb || !sr) yield break;

        float stoneWidth = sr.bounds.size.x;
        float stoneHeight = sr.bounds.size.y;

        bool success = false;
        bool isPerfectPlacement = false;

        // FIRST STONE
        if (stack.Count == 0)
        {
            Vector2 checkPos = (Vector2)stone.transform.position + Vector2.down * (stoneHeight / 2 + 0.05f);
            Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, overlapRadius);

            foreach (var h in hits)
            {
                if (h.CompareTag("Platform"))
                    success = true;
            }

            if (!success)
            {
                GameManager.Instance?.OnMiss(stone);
                yield break;
            }

            // snap on platform
            rb.bodyType = RigidbodyType2D.Static;
            stack.Add(stone);

            GameManager.Instance?.OnPlacedSuccessful(stack.Count, stone);

            DropperController.Instance?.SpawnNextStoneDelayed(0.1f);
            yield break;
        }

        // OTHER STONES
        GameObject top = stack[stack.Count - 1];
        var topSR = top.GetComponent<SpriteRenderer>();

        float topWidth = topSR.bounds.size.x;
        float topHeight = topSR.bounds.size.y;

        float dx = stone.transform.position.x - top.transform.position.x;
        float absDx = Mathf.Abs(dx);
        float dy = stone.transform.position.y - top.transform.position.y;

        bool verticallyOK = dy > -0.05f && dy < topHeight * 2f;

        float overlap = (topWidth / 2 + stoneWidth / 2) - absDx;

        if (!verticallyOK || overlap <= 0f)
        {
            GameManager.Instance?.OnMiss(stone);
            yield break;
        }

        success = true;

        if (absDx <= perfectThreshold)
            isPerfectPlacement = true;

        // snap on top
        Vector3 pos = stone.transform.position;
        pos.x = top.transform.position.x;
        pos.y = top.transform.position.y + topHeight / 2 + stoneHeight / 2;
        stone.transform.position = pos;

        stack.Add(stone);

        // --------------------------
        // PERFECT PLACEMENT → WIDEN
        // --------------------------
        if (isPerfectPlacement)
        {
            float newWidth = Mathf.Clamp(NextStoneWidth + fixedWiden, minStoneWidth, originalStoneWidth);

            // current placed stone
            ApplyWidthToStone(stone, newWidth);

            // previous stone (optional widening)
            ApplyWidthToStone(top, newWidth);

            NextStoneWidth = newWidth;

            GameManager.Instance?.OnPerfectPlacement(stack.Count, stone);
            UIManager.Instance?.ShowPerfectText();
        }
        // --------------------------
        // NOT PERFECT → SHRINK
        // --------------------------
        else
        {
            float newWidth = Mathf.Clamp(NextStoneWidth - fixedShrink, minStoneWidth, originalStoneWidth);

            // shrink placed stone
            ApplyWidthToStone(stone, newWidth);

            // shrink previous stone too
            ApplyWidthToStone(top, newWidth);

            NextStoneWidth = newWidth;

            GameManager.Instance?.OnPlacedSuccessful(stack.Count, stone);
        }

        // freeze physics
        rb.bodyType = RigidbodyType2D.Static;

        // spawn next
        DropperController.Instance?.SpawnNextStoneDelayed(0.08f);
    }

    // FIX: Scale only X. Keep Y untouched.
    private void ApplyWidthToStone(GameObject stone, float targetWorldWidth)
    {
        if (!stone) return;

        var sr = stone.GetComponent<SpriteRenderer>();
        if (!sr || !sr.sprite) return;

        float spriteNativeWidth = sr.sprite.bounds.size.x;

        float newScaleX = targetWorldWidth / spriteNativeWidth;
        float retainedScaleY = stone.transform.localScale.y; // DO NOT CHANGE HEIGHT

        stone.transform.localScale = new Vector3(newScaleX, retainedScaleY, 1f);

        // collider
        var col = stone.GetComponent<BoxCollider2D>();
        if (col)
        {
            float finalW = sr.bounds.size.x;
            float finalH = sr.bounds.size.y;
            col.size = new Vector2(finalW / newScaleX, finalH / retainedScaleY);
        }
    }

    public void ResetStack()
    {
        foreach (var o in stack)
            if (o) Destroy(o);

        stack.Clear();
        NextStoneWidth = originalStoneWidth;
    }
}
