using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public DropperController dropper;
    public BackgroundManager backgroundManager;
    public UIManager uiManager;
    public GameObject perfectPlacementEffect;
    public Transform dropperTransform;

    [Header("Progress")]
    public int score = 0;
    public int level = 1;

    [Tooltip("How many blocks required to level up")]
    public int BlocksPerLevel = 5;

    // how many placed since last level-up (0..BlocksPerLevel-1)
    private int placedSinceLevel = 0;

    public bool isGameOver = false;

    [Header("Audio")]
    public AudioClip dropSound;
    public AudioClip perfectSound;
    public AudioClip placedSound;
    public AudioClip missSound;
    private AudioSource audioSource;

    private const string SaveKey_Level = "PLAYER_LEVEL";
    private const string SaveKey_Speed = "PLAYER_SPEED";

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

        // LOAD SAVED LEVEL & SPEED
        if (PlayerPrefs.HasKey(SaveKey_Level))
        {
            level = PlayerPrefs.GetInt(SaveKey_Level);
            // On start user requested: require full BlocksPerLevel stones to reach next level.
            placedSinceLevel = 0;

            uiManager.UpdateLevel(level);

            float savedSpeed = PlayerPrefs.GetFloat(SaveKey_Speed, DropperController.Instance.baseSpeed);
            dropper.SetSpeed(savedSpeed);

            Debug.Log($"Loaded Level: {level}. Reset placedSinceLevel to {placedSinceLevel}. Speed restored: {savedSpeed}");
        }
        else
        {
            // first time
            level = 1;
            placedSinceLevel = 0;
            ui_manager_safe_update();
            dropper.UpdateSpeed(level);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetLevel();
            PlayerPrefs.SetInt(SaveKey_Level, 1);
            PlayerPrefs.SetFloat(SaveKey_Speed, 5);
            PlayerPrefs.Save();
            Debug.Log("Level reset!");
        }
    }


    // small helper to avoid accidental null-call in inspector-less situations
    private void ui_manager_safe_update()
    {
        if (uiManager != null) uiManager.UpdateLevel(level);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    public void OnDrop()
    {
        PlaySound(dropSound);
    }

    public void OnMiss(GameObject stone)
    {
        if (isGameOver) return;

        PlaySound(missSound);
        isGameOver = true;
        uiManager.ShowGameOver();

        // freeze all physics
        foreach (var rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
            rb.simulated = false;
    }

    /// <summary>
    /// Called by StackManager after a stone is placed successfully.
    /// </summary>
    public void OnPlacedSuccessful(int placedCount, GameObject stone)
    {
        // score + UI
        score += 1;
        uiManager.UpdateScore(score);
        PlaySound(placedSound);

        // move dropper / camera up
        var sr = stone.GetComponent<SpriteRenderer>();
        float stoneHeight = sr != null ? sr.bounds.size.y : 1f;
        Vector3 pos = dropperTransform.position;
        pos.y += stoneHeight;
        dropperTransform.position = pos;

        // theme update (your original logic)
        //if (placedCount % 5 == 0)
            backgroundManager.UpdateTheme(placedCount);

        // ------- LEVEL PROGRESSION -------
        placedSinceLevel++;

        if (placedSinceLevel >= BlocksPerLevel)
        {
            placedSinceLevel = 0;
            level++;

            // SAVE LEVEL *IMMEDIATELY* BEFORE ANYTHING ELSE
            PlayerPrefs.SetInt(SaveKey_Level, level);
            PlayerPrefs.Save();

            uiManager.UpdateLevel(level);
            //uiManager.ShowLevelUp(level);  /////////////////////////////////////////////// need to do////////////////////////////////////////////////

            // update speed AFTER save
            dropper.UpdateSpeed(level);

            // save speed
            PlayerPrefs.SetFloat(SaveKey_Speed, dropper.CurrentSpeed);
            PlayerPrefs.Save();

            Debug.Log($"Level UP → {level}  Saved.");
        }
        else
        {
            // normal save
            PlayerPrefs.SetInt(SaveKey_Level, level);
            PlayerPrefs.SetFloat(SaveKey_Speed, dropper.CurrentSpeed);
            PlayerPrefs.Save();
        }
    }

    public void OnPerfectPlacement(int lvl, GameObject stone)
    {
        PlaySound(perfectSound);
        AddScore(10);

        // Move dropper up
        if (perfectPlacementEffect != null)
        {
            Vector3 pos = stone.transform.position + Vector3.up * 0.5f;
            GameObject fx = Instantiate(perfectPlacementEffect, pos, Quaternion.identity);

            var sr = stone.GetComponent<SpriteRenderer>();
            float stoneHeight = sr != null ? sr.bounds.size.y : 1f;

            Vector3 pos1 = dropperTransform.position;
            pos1.y += stoneHeight;
            dropperTransform.position = pos1;

            Destroy(fx, 2f);
        }

        Debug.Log("🌟 PERFECT PLACEMENT! Bonus +10 points");

        // --- THIS WAS MISSING ---
        // Perfect placement MUST ALSO count toward level progression
        placedSinceLevel++;

        if (placedSinceLevel >= BlocksPerLevel)
        {
            placedSinceLevel = 0;
            level++;

            // Save level immediately
            PlayerPrefs.SetInt(SaveKey_Level, level);
            PlayerPrefs.Save();

            uiManager.UpdateLevel(level);
            //uiManager.ShowLevelUp(level);  /////////////////////////////////////////////// need to do////////////////////////////////////////////////

            // update speed
            dropper.UpdateSpeed(level);

            // save speed
            PlayerPrefs.SetFloat(SaveKey_Speed, dropper.CurrentSpeed);
            PlayerPrefs.Save();

            Debug.Log($"(Perfect) Level UP → {level}  Saved.");
        }
        else
        {
            // Normal save (even without leveling)
            PlayerPrefs.SetInt(SaveKey_Level, level);
            PlayerPrefs.SetFloat(SaveKey_Speed, dropper.CurrentSpeed);
            PlayerPrefs.Save();
        }
    }

    // for testing
    public void ResetLevel()
    {
        level = 1;                  
        UIManager.Instance.UpdateLevel(level);
    }


    public void AddScore(int amount)
    {
        score += amount;
        uiManager.UpdateScore(score);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
