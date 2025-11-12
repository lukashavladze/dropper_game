using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    public DropperController dropper;
    public BackgroundManager backgroundManager;
    public UIManager uiManager;

    // newly added
    public Transform dropperTransform; // assign to inspector
    public float verticalStep = 1.0f; // how high to move per cube.
    // aqamde
    public int score = 0;
    public int level = 1;
    public int basePlacedToLevelUp = 5;

    public float speedIncreasePerLevel = 0.6f;
    public int placedToLevelUp = 5;


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


    public void OnMiss(GameObject stone)
    {
        // miss: end run (or subtract life). For simplicity: end game
        Destroy(stone);
        uiManager.ShowGameOver();
    }

    public void OnPlacedSuccessful(int placedCount, GameObject stone)
    {
        // Increase score
        score += 10;
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
    // to dynamically increase lvl
    int GetPlacedToLevelUp()
    {
        // Each level reduces the required blocks slightly (never below 3)
        return Mathf.Max(3, basePlacedToLevelUp - (level / 2));
    }



    public void Restart()
    {
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