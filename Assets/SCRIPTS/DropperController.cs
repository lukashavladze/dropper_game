using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DropperController : MonoBehaviour
{
    public GameObject stonePrefab;
    public Transform spawnPoint;
    public float leftX = -5f, rightX = 5f;
    public float baseSpeed = 2f;
    public float speedIncreasePerLevel = 0.3f;

    public float CurrentSpeed => moveSpeed;

    private GameObject currentStone;
    public GameObject CurrentStone => currentStone;

    private int direction = 1;
    public static DropperController Instance;

    public Sprite currentStoneSkin;
    private float moveSpeed;

    [Header("Stone Skins")]
    public Sprite[] stoneSprites;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        moveSpeed = baseSpeed;

        if (SkinSelection.SelectedStoneSkin != null)
            SetStoneSkin(SkinSelection.SelectedStoneSkin);

        SpawnStone();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
        if (PauseManager.IsPaused) return;

        Move();

        // Ignore input if over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        var mouse = Mouse.current;
        var touch = Touchscreen.current;

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            DropCurrent();
        }
        else if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
        {
            DropCurrent();
        }
    }


    public void IncreaseSpeedSmall()
    {
        baseSpeed += 0.15f;
        moveSpeed = baseSpeed;
        Debug.Log($"🚀 SPEED INCREASE → baseSpeed={baseSpeed}, moveSpeed={moveSpeed}");
    }

    void Move()
    {
        float move = moveSpeed * Time.deltaTime;
        transform.position += Vector3.right * direction * move;
        if (transform.position.x > rightX) direction = -1;
        if (transform.position.x < leftX) direction = 1;

            if (currentStone)
            {
                var sr = currentStone.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    float halfHeight = sr.bounds.size.y * 0.1f;

                    currentStone.transform.position = new Vector3(
                        spawnPoint.position.x,
                        spawnPoint.position.y - halfHeight, // 🔥 align top
                        spawnPoint.position.z
                    );
                }
            }
    }

    public void SpawnStone()
    {
        // Safety: don't spawn if there is an active stone already
        if (currentStone != null) return;

        currentStone = Instantiate(stonePrefab, spawnPoint.position, Quaternion.identity);

        // Apply skin if set
        var sr2 = currentStone.GetComponent<SpriteRenderer>();

        if (sr2 != null && stoneSprites.Length > 0)
        {
            int index = Random.Range(0, stoneSprites.Length);
            sr2.sprite = stoneSprites[index];

            // 🔥 store color for later (VERY IMPORTANT)
            currentStoneSkin = stoneSprites[index];
        }

        var sr = currentStone.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float nativeWidth = sr.sprite.bounds.size.x;
            float desiredWorldWidth = StackManager.Instance != null ? StackManager.Instance.NextStoneWidth : 1f;
            if (nativeWidth > 0.001f)
            {
                float k = desiredWorldWidth / nativeWidth;
                currentStone.transform.localScale = new Vector3(k, k, 1f);
            }
        }

        var col = currentStone.GetComponent<BoxCollider2D>();
        if (sr != null && col != null)
        {
            var ls = currentStone.transform.localScale;
            col.size = new Vector2(sr.bounds.size.x / ls.x, sr.bounds.size.y / ls.y);
            col.offset = Vector2.zero;
        }

        var rb = currentStone.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        var fo = currentStone.GetComponent<FallingObject>();
        if (fo != null) fo.OnPlaced += OnStonePlaced;
    }

    public void SpawnNextStoneDelayed(float delay)
    {
        StartCoroutine(SpawnNextAfterDelay(delay));
    }
    private IEnumerator SpawnNextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentStone == null) SpawnStone();
    }

    void DropCurrent()
    {
        if (!currentStone) return;

        GameObject fallingStone = currentStone; // keep reference

        var rb = fallingStone.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;

        fallingStone.transform.parent = null;

        GameManager.Instance?.OnDrop();

        // Register watcher that will call immediate game-over if it falls too low
        StartCoroutine(StackManager.Instance.CheckMissWhileFalling(fallingStone));

        // Clear the current stone so SpawnNextAfterDelay can create next
        currentStone = null;

        // Ensure next stone will appear after a short delay
        //StartCoroutine(SpawnNextAfterDelay(0.8f));
    }

    private void OnStonePlaced(GameObject stone)
    {
        // Reset rotation to perfectly straight (no tilt)
        stone.transform.rotation = Quaternion.identity;

        // Continue with your existing logic
        StackManager.Instance.RegisterPlacedStone(stone);

        var fo = stone.GetComponent<FallingObject>();
        if (fo != null) fo.OnPlaced -= OnStonePlaced;
    }

    // SKIN API
    public void SetStoneSkin(Sprite newSkin)
    {
        currentStoneSkin = newSkin;
        if (currentStone != null)
        {
            var sr = currentStone.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = newSkin;
        }
    }
}
