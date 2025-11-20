using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private float platformY;

    [Header("Game Over / Live check settings")]
    public bool requireHorizontalOverlapForGameOver = false;
    public float gameOverVerticalMargin = 0.02f;

    public float NextStoneWidth { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        NextStoneWidth = originalStoneWidth;
    }

    void Start()
    {
        GameObject platform = GameObject.FindGameObjectWithTag("Platform");
        if (platform != null)
        {
            var sr = platform.GetComponent<SpriteRenderer>();
            platformY = platform.transform.position.y + (sr != null ? sr.bounds.size.y * 0.5f : 0.5f);
        }
    }

    public void RegisterPlacedStone(GameObject stone)
    {
        StartCoroutine(CheckPlacementNextFrame(stone));
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.isGameOver) return;
        if (stack.Count == 0) return;
        if (DropperController.Instance == null) return;

        GameObject falling = DropperController.Instance.CurrentStone;
        if (falling == null) return;

        Rigidbody2D rb = falling.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        if (!rb.simulated) return;
        if (rb.linearVelocity.y >= 0f) return;

        var srFall = falling.GetComponent<SpriteRenderer>();
        var lastPlaced = stack[stack.Count - 1];
        var srLast = lastPlaced.GetComponent<SpriteRenderer>();
        if (srFall == null || srLast == null) return;

        float fallingBottom = falling.transform.position.y - srFall.bounds.size.y * 0.5f;
        float lastTop = lastPlaced.transform.position.y + srLast.bounds.size.y * 0.5f;

        if (fallingBottom < lastTop - gameOverVerticalMargin)
        {
            if (requireHorizontalOverlapForGameOver)
            {
                float fallHalfW = srFall.bounds.size.x * 0.5f;
                float lastHalfW = srLast.bounds.size.x * 0.5f;
                float dx = Mathf.Abs(falling.transform.position.x - lastPlaced.transform.position.x);
                float horizOverlap = (fallHalfW + lastHalfW) - dx;
                if (horizOverlap <= 0f)
                    GameManager.Instance.OnMiss(falling);
            }
            else
            {
                GameManager.Instance.OnMiss(falling);
            }
        }
    }

    private IEnumerator CheckPlacementNextFrame(GameObject stone)
    {
        yield return null;
        if (stone == null) yield break;

        Rigidbody2D rb = stone.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = stone.GetComponent<SpriteRenderer>();
        if (!rb || !sr) yield break;

        float stoneWidth = sr.bounds.size.x;
        float stoneHeight = sr.bounds.size.y;

        // FIRST STONE
        if (stack.Count == 0)
        {
            GameObject platform = GameObject.FindGameObjectWithTag("Platform");
            if (platform == null) { GameManager.Instance?.OnMiss(stone); yield break; }

            float stoneBottom = stone.transform.position.y - stoneHeight * 0.5f;
            float platformTop = platform.transform.position.y + (platform.GetComponent<SpriteRenderer>() != null
                ? platform.GetComponent<SpriteRenderer>().bounds.size.y * 0.5f
                : 0.5f);

            if (Mathf.Abs(stoneBottom - platformTop) > 0.4f)
            {
                GameManager.Instance?.OnMiss(stone);
                yield break;
            }

            rb.bodyType = RigidbodyType2D.Static;
            stack.Add(stone);
            GameManager.Instance?.OnPlacedSuccessful(stack.Count, stone);

            // ensure next stone spawns
            DropperController.Instance?.SpawnNextStoneDelayed(0.1f);
            yield break;
        }

        // LATER STONES
        GameObject top = stack[stack.Count - 1];
        var topSR = top.GetComponent<SpriteRenderer>();
        if (topSR == null) { GameManager.Instance?.OnMiss(stone); yield break; }

        float topWidth = topSR.bounds.size.x;
        float topHeight = topSR.bounds.size.y;

        float dx = stone.transform.position.x - top.transform.position.x;
        float absDx = Mathf.Abs(dx);

        float overlap = (topWidth * 0.5f + stoneWidth * 0.5f) - absDx;
        if (overlap <= 0f)
        {
            GameManager.Instance?.OnMiss(stone);
            yield break;
        }

        bool isPerfect = absDx <= perfectThreshold;

        // Snap on top
        float topTop = top.transform.position.y + topHeight * 0.5f;
        stone.transform.position = new Vector3(top.transform.position.x, topTop + stoneHeight * 0.5f, stone.transform.position.z);

        // freeze stone and add
        rb.bodyType = RigidbodyType2D.Static;
        stack.Add(stone);

        // width logic
        if (isPerfect)
        {
            float newWidth = Mathf.Clamp(NextStoneWidth + fixedWiden, minStoneWidth, originalStoneWidth);
            ApplyWidthToStone(stone, newWidth);
            ApplyWidthToStone(top, newWidth);
            NextStoneWidth = newWidth;

            GameManager.Instance?.OnPerfectPlacement(stack.Count, stone);
            UIManager.Instance?.ShowPerfectText();
        }
        else
        {
            float newWidth = Mathf.Clamp(NextStoneWidth - fixedShrink, minStoneWidth, originalStoneWidth);
            ApplyWidthToStone(stone, newWidth);
            ApplyWidthToStone(top, newWidth);
            NextStoneWidth = newWidth;

            GameManager.Instance?.OnPlacedSuccessful(stack.Count, stone);
        }

        // ensure next spawn (safety)
        DropperController.Instance?.SpawnNextStoneDelayed(0.08f);
    }

    public IEnumerator CheckMissWhileFalling(GameObject stone)
    {
        float referenceY = (stack.Count == 0)
            ? platformY
            : stack[stack.Count - 1].transform.position.y;

        while (stone != null)
        {
            if (stone.transform.position.y < referenceY - 1f)
            {
                Debug.Log("❌ Stone fell below stack — instant game over");
                GameManager.Instance?.OnMiss(stone);
                yield break;
            }

            yield return null;
        }
    }

    private void ApplyWidthToStone(GameObject stone, float targetWorldWidth)
    {
        if (stone == null) return;
        var sr = stone.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        float nativeWidth = sr.sprite.bounds.size.x;
        if (nativeWidth <= 0.0001f) return;

        float newScaleX = targetWorldWidth / nativeWidth;
        float keepY = stone.transform.localScale.y;

        stone.transform.localScale = new Vector3(newScaleX, keepY, 1f);

        var col = stone.GetComponent<BoxCollider2D>();
        if (col != null)
        {
            var ls = stone.transform.localScale;
            float worldW = sr.bounds.size.x;
            float worldH = sr.bounds.size.y;
            col.size = new Vector2(worldW / ls.x, worldH / ls.y);
            col.offset = Vector2.zero;
        }
    }

    public GameObject GetLastStone() => stack.Count == 0 ? null : stack[stack.Count - 1];
    public int GetStackCount() => stack.Count;
}
