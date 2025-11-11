using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    public DropperController dropper;
    public BackgroundManager backgroundManager;
    public UIManager uiManager;


    public int score = 0;
    public int level = 1;


    public float speedIncreasePerLevel = 0.6f;
    public int placedToLevelUp = 10;


    void Awake() { Instance = this; }


    void Start()
    {
        uiManager.UpdateScore(score);
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
        score += 10; // points per stone
        uiManager.UpdateScore(score);
        backgroundManager.UpdateTheme(placedCount);


        // level progression
        int newLevel = 1 + (placedCount / placedToLevelUp);
        if (newLevel > level)
        {
            level = newLevel;
        }
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