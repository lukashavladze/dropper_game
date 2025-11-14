using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public DropperController dropper;
    public BackgroundManager backgroundManager;
    public UIManager uiManager;
    public GameObject perfectPlacementEffect;

    public Transform dropperTransform;
    public float verticalStep = 1f;

    public int score = 0;
    public int level = 1;

    public int placedCount = 0;               // MAIN COUNTER
    public int basePlacedToLevelUp = 5;

    public bool isGameOver = false;

    // Sounds
    public AudioClip dropSound;
    public AudioClip perfectSound;
    public AudioClip placedSound;
    public AudioClip missSound;
    private AudioSource audioSource;

    // Saved state for continue
    private Vector3 lastDropperPos;
    private int lastPlacedCount;
    private int lastScore;
    private int lastLevel;
    private Vector3 lastStonePos;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        uiManager.UpdateScore(score);

        var camFollow = FindFirstObjectByType<CameraFollow>();
        if (camFollow != null && dropperTransform != null)
            camFollow.target = dropperTransform;
    }

    // ------------------- SOUND -------------------

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    // ------------------- DROP -------------------

    public void OnDrop()
    {
        PlaySound(dropSound);
    }

    // ------------------- MISS / FAIL -------------------

    public void OnMiss(GameObject stone)
    {
        if (isGameOver) return;

        PlaySound(missSound);
        isGameOver = true;

        uiManager.ShowGameOver();

        foreach (var rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
            rb.simulated = false;
    }

    // ------------------- SUCCESS -------------------

    public void OnPlacedSuccessful(GameObject stone)
    {
        placedCount++;
        score++;
        uiManager.UpdateScore(score);

        // SAVE LAST GOOD STATE
        lastPlacedCount = placedCount;
        lastScore = score;
        lastLevel = level;
        lastStonePos = stone.transform.position;
        lastDropperPos = dropperTransform.position;

        PlaySound(placedSound);

        // move dropper up
        dropperTransform.position += new Vector3(0, verticalStep, 0);

        // Change background every 5
        if (placedCount % 5 == 0)
            backgroundManager.UpdateTheme(placedCount);

        // Level progression
        int lvlUpEvery = GetPlacedToLevelUp();
        int newLevel = 1 + (placedCount / lvlUpEvery);
        if (newLevel > level)
        {
            level = newLevel;
            uiManager.UpdateLevel(level);
            dropper.UpdateSpeed(level);
        }
    }

    public void OnPerfectPlacement(GameObject stone)
    {
        PlaySound(perfectSound);

        AddScore(10);

        if (perfectPlacementEffect != null)
        {
            var pos = stone.transform.position + Vector3.up * .5f;
            var fx = Instantiate(perfectPlacementEffect, pos, Quaternion.identity);
            Destroy(fx, 2f);

            dropperTransform.position += new Vector3(0, verticalStep, 0);
        }
    }

    // ------------------- HARD LOGIC -------------------

    int GetPlacedToLevelUp()
    {
        return Mathf.Max(3, basePlacedToLevelUp - (level / 2));
    }

    public void AddScore(int amount)
    {
        score += amount;
        uiManager.UpdateScore(score);
    }

    // ------------------- RESTART -------------------

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ------------------- CONTINUE -------------------

    public void ContinueGame()
    {
        if (!isGameOver)
            return;

        isGameOver = false;

        // Restore saved values
        placedCount = lastPlacedCount;
        score = lastScore;
        level = lastLevel;

        dropperTransform.position = lastDropperPos;

        uiManager.UpdateScore(score);
        uiManager.UpdateLevel(level);

        // Remove falling stones
        foreach (var rb in FindObjectsOfType<Rigidbody2D>())
        {
            if (rb.simulated == false)       // frozen ones from game over
                Destroy(rb.gameObject);
        }

        uiManager.HideGameOver();

        dropper.enabled = true;

        // optional: manually call new stone spawn
        if (dropper != null)
            dropper.SpawnNextStone();
    }
}
