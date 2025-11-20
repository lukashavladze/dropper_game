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
    public GameObject CurrentStone => currentStone; // safe public getter

    private int direction = 1;
    public static DropperController Instance;

    public Sprite currentStoneSkin;
    private float moveSpeed;

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

        // Ignore input if clicking on UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
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

    public void SpawnStone()
    {
        currentStone = Instantiate(stonePrefab, spawnPoint.position, Quaternion.identity);

        // apply selected skin
        if (currentStoneSkin != null)
        {
            var sr2 = currentStone.GetComponent<SpriteRenderer>();
            if (sr2 != null) sr2.sprite = currentStoneSkin;
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

        // update collider to match scale
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
        SpawnStone();
    }

    void DropCurrent()
    {
        if (!currentStone) return;

        GameObject fallingStone = currentStone;   // store reference before nulling it

        var rb = fallingStone.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;

        fallingStone.transform.parent = null;
        GameManager.Instance?.OnDrop();

        // Now pass correct stone reference
        StartCoroutine(StackManager.Instance.CheckMissWhileFalling(currentStone));

        currentStone = null;
    }


    private void OnStonePlaced(GameObject stone)
    {
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
