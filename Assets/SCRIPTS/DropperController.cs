using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropperController : MonoBehaviour
{
    public GameObject stonePrefab;
    public Transform spawnPoint;
    public float leftX = -5f, rightX = 5f;
    public float baseSpeed = 5f;
    public float speedIncreasePerLevel = 1.5f;

    private GameObject currentStone;
    private int direction = 1;
    public static DropperController Instance;

    public Sprite currentStoneSkin;
    private float moveSpeed;

    void Awake()
    {
        Instance = this;
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
        var gm = GameManager.Instance;
        if (gm != null && gm.isGameOver) return;
        if (PauseManager.IsPaused) return;

        Move();

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0) ||
           (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began))
        {
            DropCurrent();
        }
    }

    public void UpdateSpeed(int level)
    {
        moveSpeed = baseSpeed + (level - 1) * speedIncreasePerLevel;
    }

    void Move()
    {
        float move = moveSpeed * Time.deltaTime;
        transform.position += Vector3.right * direction * move;

        if (transform.position.x > rightX) direction = -1;
        if (transform.position.x < leftX) direction = 1;

        if (currentStone)
            currentStone.transform.position = spawnPoint.position;
    }

    // ----------------------------------------------------------
    //          SPAWN SYSTEM
    // ----------------------------------------------------------
    public void SpawnStone()
    {
        currentStone = Instantiate(stonePrefab, spawnPoint.position, Quaternion.identity);

        SpriteRenderer sr = currentStone.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float nativeWidth = sr.sprite.bounds.size.x;
            float desiredWorldWidth = StackManager.Instance != null
                ? StackManager.Instance.NextStoneWidth
                : 1f;

            if (nativeWidth > 0.001f)
            {
                float k = desiredWorldWidth / nativeWidth;
                currentStone.transform.localScale = new Vector3(k, k, 1f);
            }
        }

        // Fix collider after scaling
        BoxCollider2D col = currentStone.GetComponent<BoxCollider2D>();
        if (sr != null && col != null)
        {
            var ls = currentStone.transform.localScale;
            col.size = new Vector2(sr.bounds.size.x / ls.x, sr.bounds.size.y / ls.y);
            col.offset = Vector2.zero;
        }

        // RigidBody off until dropped
        Rigidbody2D rb = currentStone.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;

        // Subscribe to placement event
        FallingObject fo = currentStone.GetComponent<FallingObject>();
        if (fo != null)
            fo.OnPlaced += OnStonePlaced;
    }

    public void SpawnNextStoneDelayed(float delay)
    {
        StartCoroutine(SpawnNextAfterDelay(delay));
    }

    private IEnumerator SpawnNextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnStone();
    }

    // ----------------------------------------------------------
    //          DROP SYSTEM
    // ----------------------------------------------------------
    void DropCurrent()
    {
        if (!currentStone) return;

        var rb = currentStone.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;

        currentStone.transform.parent = null;
        GameManager.Instance?.OnDrop();
        currentStone = null; // clear reference
    }

    private void OnStonePlaced(GameObject stone)
    {
        StackManager.Instance.RegisterPlacedStone(stone);

        FallingObject fo = stone.GetComponent<FallingObject>();
        if (fo != null)
            fo.OnPlaced -= OnStonePlaced; // prevent double-calls
    }

    // ----------------------------------------------------------
    //          SKINS
    // ----------------------------------------------------------
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
