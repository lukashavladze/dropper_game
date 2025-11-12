using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    public DropperController dropper;
    public BackgroundManager backgroundManager;
    public UIManager uiManager;
    public GameObject perfectPlacementEffect;

    // newly added
    public Transform dropperTransform; // assign to inspector
    public float verticalStep = 1.0f; // how high to move per cube.
    // aqamde
    public int score = 0;
    public int level = 1;
    public int basePlacedToLevelUp = 5;

    public float speedIncreasePerLevel = 0.6f;
    public int placedToLevelUp = 5;
    public bool isGameOver = false;


    void Awake() { Instance = this; }


    void Start()
    {
        uiManager.UpdateScore(score);
        var camFollow = FindFirstObjectByType<CameraFollow>();
        if (camFollow != null && dropperTransform != null)
            camFollow.target = dropperTransform;
    }


    public void OnDrop()
    {
        // called when player drops — can add sound or analytics
    }


    //public void OnMiss(GameObject stone)
    //{
    //    // miss: end run (or subtract life). For simplicity: end game
    //    Destroy(stone);
    //    uiManager.ShowGameOver();
    //}

    public void OnMiss(GameObject stone)
    {
        if (isGameOver) return; // prevent multiple triggers

        isGameOver = true;
        uiManager.ShowGameOver();

        // Optional: freeze all stones
        foreach (var rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
        {
            rb.simulated = false;
        }
    }

    public void OnPlacedSuccessful(int placedCount, GameObject stone)
    {
        // Increase score
        score += 1;
        uiManager.UpdateScore(score);

        // Move dropper upward (camera will follow automatically)
        Vector3 pos = dropperTransform.position;
        pos.y += verticalStep;
        dropperTransform.position = pos;

        if (placedCount % 5 == 0) // change background every 5 cubes
        {
            backgroundManager.UpdateTheme(placedCount);
        }

        // Handle level progression dynamically
        int placedToLevelUp = GetPlacedToLevelUp();
        int newLevel = 1 + (placedCount / placedToLevelUp);
        if (newLevel > level)
        {
            level = newLevel;
            uiManager.UpdateLevel(newLevel);

            // increase difficulty
            dropper.UpdateSpeed(level);

            // <<<<<<< to apply gravity speed >>>>>>>
            //foreach (var s in GameObject.FindGameObjectsWithTag("Stone"))
            //{
            //    var falling = s.GetComponent<FallingObject>();
            //    if (falling != null)
            //        falling.UpdateGravity(level); // reapply gravity settings dynamically
            //}
        }
    }

    public void OnPerfectPlacement(int level, GameObject stone)
    {
        // Bonus logic
        AddScore(10);

        // Particle effect
        if (perfectPlacementEffect != null)
        {
            Vector3 pos = stone.transform.position + Vector3.up * 0.5f;
            GameObject fx = Instantiate(perfectPlacementEffect, pos, Quaternion.identity);
            // dropper to follow upwards after perfect placement
            Vector3 pos1 = dropperTransform.position;
            pos1.y += verticalStep;
            dropperTransform.position = pos1;
            Destroy(fx, 2f); // auto-remove after 2 seconds
        }

        Debug.Log("🌟 PERFECT PLACEMENT! Bonus +10 points");
    }

    // to dynamically increase lvl
    int GetPlacedToLevelUp()
    {
        // Each level reduces the required blocks slightly (never below 3)
        return Mathf.Max(3, basePlacedToLevelUp - (level / 2));
    }

    public void AddScore(int amount)
    {
        score += amount;
        uiManager.UpdateScore(score);
    }



    public void Restart()
    {
        isGameOver = false;
        score = 0;
        level = 1;
        uiManager.UpdateScore(score);
        uiManager.HideGameOver();
        StackManager.Instance.ResetStack();
        dropper.transform.position = new Vector3(0, dropper.transform.position.y, dropper.transform.position.z);
        dropper.speed = Mathf.Abs(dropper.speed); // optional reset
        backgroundManager.UpdateTheme(0);
        dropper.SendMessage("SpawnStone");
    }
}