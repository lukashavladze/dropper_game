using UnityEngine;


public class DropperController : MonoBehaviour
{
    public GameObject stonePrefab; // assign prefab
    public Transform spawnPoint; // where stones are created
    public float speed = 3f; // horizontal movement speed
    public float leftX = -5f, rightX = 5f; // bounds


    private GameObject currentStone;
    private int direction = 1;


    void Start()
    {
        SpawnStone();
    }


    void Update()
    {
        Move();
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            DropCurrent();
        }
    }


    void Move()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
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
        var rb = currentStone.GetComponent<Rigidbody2D>();
        rb.simulated = true;
        // detach so next spawn won't move it
        currentStone.transform.parent = null;
        currentStone = null;
        // spawn immediate replacement so it moves while previous falls
        SpawnStone();
        GameManager.Instance.OnDrop();
    }


    void OnStonePlaced(GameObject stone)
    {
        // forwarded from FallingObject when it settles
        StackManager.Instance.RegisterPlacedStone(stone);
    }
}