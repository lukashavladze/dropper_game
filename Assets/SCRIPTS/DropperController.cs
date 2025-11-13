using UnityEngine;
using UnityEngine.EventSystems;


public class DropperController : MonoBehaviour
{
    public GameObject stonePrefab; // assign prefab
    public Transform spawnPoint; // where stones are created
    public float speed = 3f; // horizontal movement speed
    public float leftX = -5f, rightX = 5f; // bounds
    public float speedIncreasePerLevel = 1.5f;
    public float moveSpeed = 5f;
    public float baseSpeed = 5f;


    private GameObject currentStone;
    private int direction = 1;


    void Start()
    {
        SpawnStone();
    }


    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver || PauseManager.IsPaused)
            return; // stop all dropper behavior

        Move();

        // Ignore input if clicking on UI (pause button etc.)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            DropCurrent();
        }
    }

    public void UpdateSpeed(int level)
    {
        moveSpeed = baseSpeed + (level - 1) * speedIncreasePerLevel;
        Debug.Log($"Dropper speed updated to {moveSpeed} at level {level}");
    }


    void Move()
    {
        float move = moveSpeed * Time.deltaTime;
        //transform.position += Vector3.right * direction * speed * Time.deltaTime;
        transform.position += Vector3.right * direction * move;
        if (transform.position.x > rightX) { direction = -1; }
        if (transform.position.x < leftX) { direction = 1; }
        if (currentStone) // follow dropper
        {
            currentStone.transform.position = spawnPoint.position;
        }
    }


    void SpawnStone()
    {
        currentStone = Instantiate(stonePrefab, spawnPoint.position, Quaternion.identity);
        var fo = currentStone.GetComponent<FallingObject>();
        if (fo != null) fo.OnPlaced += OnStonePlaced;
    }


    void DropCurrent()
    {
        if (!currentStone) return;
        GameObject fallingStone = currentStone;
        var rb = fallingStone.GetComponent<Rigidbody2D>();
        //var rb = currentStone.GetComponent<Rigidbody2D>();
        rb.simulated = true;
        // detach so next spawn won't move it
        currentStone.transform.parent = null;
        //currentStone = null;
        // spawn immediate replacement so it moves while previous falls
        SpawnStone();
        GameManager.Instance.OnDrop();
        StartCoroutine(StackManager.Instance.CheckMissWhileFalling(currentStone));
    }


    void OnStonePlaced(GameObject stone)
    {
        // forwarded from FallingObject when it settles
        StackManager.Instance.RegisterPlacedStone(stone);
    }
}