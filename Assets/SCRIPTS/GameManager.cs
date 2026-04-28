using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private int placedSinceLevel = 0;
    public bool isGameOver = false;

    [Header("Audio")]
    public AudioClip dropSound;
    public AudioClip perfectSound;
    public AudioClip placedSound;
    public AudioClip missSound;
    private AudioSource audioSource;

    private const string SaveKey_Level = "PLAYER_LEVEL";
    public GameObject scorePopupPrefab;

    [SerializeField] GameObject perfectFlashPrefab;

    private int blocksSinceSpeedIncrease = 0;

    // 🔥 COMBO SYSTEM
    private int comboCount = 1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        uiManager.UpdateScore(score);

        var camFollow = FindFirstObjectByType<CameraFollow>();
        if (camFollow != null && dropperTransform != null)
            camFollow.target = dropperTransform;

        if (PlayerPrefs.HasKey(SaveKey_Level))
        {
            level = PlayerPrefs.GetInt(SaveKey_Level);
            placedSinceLevel = 0;

            uiManager.UpdateLevel(level);

        }
        else
        {
            level = 1;
            placedSinceLevel = 0;
            uiManager.UpdateLevel(level);
        }
    }

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ResetEverythingForTesting();
        }
        
    }

    // =========================
    // 🔥 COMBO LOGIC
    // =========================

    void ResetCombo()
    {
        comboCount = 1;
        UIManager.Instance?.UpdateCombo(1, 1);
    }

    int GetComboMultiplier()
    {
        return Mathf.Clamp(comboCount, 1, 10); // cap at x10
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

        isGameOver = true;
        PlaySound(missSound);

        UIManager.Instance.UpdatePlanet(level);
        UIManager.Instance.UpdateScoreGameover(score);

        int best = PlayerPrefs.GetInt("BEST_SCORE", 0);
        if (score > best)
        {
            PlayerPrefs.SetInt("BEST_SCORE", score);
            PlayerPrefs.Save();
        }

        LeaderboardManager.Instance.SaveScore(score);

        uiManager.ShowGameOver();

        foreach (var rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
            rb.simulated = false;
    }

    // =========================
    // NORMAL PLACEMENT
    // =========================

    void SpawnScorePopup(Vector3 pos, int amount)
    {
        if (scorePopupPrefab == null) return;
        if (isGameOver) return;

        // 🔥 LEFT SIDE SPAWN
        Vector3 spawnPos = pos + new Vector3(-0.6f, 0.3f, -1f);

        GameObject go = Instantiate(scorePopupPrefab, spawnPos, Quaternion.identity);

        var popup = go.GetComponent<ScorePopup>();
        if (popup != null)
            popup.Setup(amount);
    }

    public void OnPlacedSuccessful(int placedCount, GameObject stone)
    {
        // ❗ RESET COMBO
        ResetCombo();
        if (placedCount > 1)
        {
            CameraShake.ShakeSafe(0.10f, 0.15f);
        }

        score += 1;
        uiManager.UpdateScore(score);
        SpawnScorePopup(stone.transform.position, 1); 
        PlaySound(placedSound);

        MoveDropperUp(stone);

        backgroundManager.UpdateTheme(level);

        HandleBlockPlaced();
        StackManager.Instance.WorsenPrecision();
        HandleLevelProgression();
    }

    // =========================
    // PERFECT PLACEMENT
    // =========================

    public void OnPerfectPlacement(int lvl, GameObject stone)
    {
        PlaySound(perfectSound);

        // 🔥 COMBO INCREASE
        if (comboCount <= 9)
        {
            comboCount++;
        }
        

        int multiplier = GetComboMultiplier();

        int bonus = Mathf.Max(1, comboCount * comboCount);

        AddScore(bonus);
        SpawnScorePopup(stone.transform.position, bonus);

        UIManager.Instance?.UpdateCombo(comboCount, multiplier);

        StartCoroutine(PerfectBounce(stone.transform));

        MoveDropperUp(stone);

        // =========================
        // 🔥 PERFECT FLASH FIXED
        // =========================
        if (perfectFlashPrefab != null)
        {
            var sr = stone.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Bounds b = sr.bounds;

                Vector3 pos = new Vector3(
                    b.center.x,
                    b.min.y + 0.01f, // tiny offset so it's visible
                    stone.transform.position.z - 0.1f
                );

                float width = b.size.x;

                GameObject fx = Instantiate(perfectFlashPrefab, pos, Quaternion.identity);

                PerfectFlash flash = fx.GetComponent<PerfectFlash>();
                if (flash != null)
                {
                    flash.Init(width);
                }
            }
        }
        if (comboCount >= 5)
        {
            StackManager.Instance.ImprovePrecision();
        }


        Debug.Log($"🌟 PERFECT x{multiplier} (combo {comboCount})");

        HandleBlockPlaced();

        HandleLevelProgression();

    }


    public int GetCombo()
    {
        return comboCount;
    }

    private void HandleBlockPlaced()
    {
        blocksSinceSpeedIncrease++;

        if (blocksSinceSpeedIncrease >= 3)
        {
            blocksSinceSpeedIncrease = 0;

            if (DropperController.Instance != null)
            {
                DropperController.Instance.IncreaseSpeedSmall();
            }
        }
    }
    public void OnNormalPlacement(int placedCount, GameObject stone)
    {
        // ❗ DO NOT RESET COMBO
        // ❗ DO NOT INCREASE COMBO

        int bonus = Mathf.Max(1, comboCount * comboCount);

        AddScore(bonus);
        SpawnScorePopup(stone.transform.position, bonus);

        PlaySound(placedSound);
        

        MoveDropperUp(stone);
        backgroundManager.UpdateTheme(level);

        HandleLevelProgression();

        HandleBlockPlaced();
    }

    // =========================
    // LEVEL SYSTEM
    // =========================

    void HandleLevelProgression()
    {
        placedSinceLevel++;

        int required = GetBlocksRequiredForLevel(level);

        if (placedSinceLevel >= required)
        {
            placedSinceLevel = 0;
            level++;

            PlayerPrefs.SetInt(SaveKey_Level, level);
            PlayerPrefs.Save();

            uiManager.UpdateLevel(level);
            uiManager.ShowLevelUp(level);

            PlayerPrefs.Save();

            Debug.Log($"Level UP → {level}");

            // 🔥 reset progress AFTER level up
            uiManager.UpdateProgress(0f);
        }

        // 🔥 ALWAYS update progress (this is the key fix)
        uiManager.UpdateProgress(GetLevelProgress01());
    }

    int GetBlocksRequiredForLevel(int lvl)
    {
        if (lvl == 1) return 50;
        if (lvl == 2) return 80;

        return 80 + (lvl - 2) * 30;
    }

    // =========================
    // HELPERS
    // =========================

    void MoveDropperUp(GameObject stone)
    {
        var sr = stone.GetComponent<SpriteRenderer>();
        float h = sr != null ? sr.bounds.size.y : 1f;

        Vector3 pos = dropperTransform.position;
        pos.y += h;
        dropperTransform.position = pos;
    }

    private IEnumerator PerfectBounce(Transform stone)
    {
        Vector3 start = stone.localScale;
        Vector3 big = start * 1.15f;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            stone.localScale = Vector3.Lerp(start, big, t);
            yield return null;
        }

        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            stone.localScale = Vector3.Lerp(big, start, t);
            yield return null;
        }
    }

    public void ContinueGame()
    {
        if (!isGameOver) return;

        isGameOver = false;

        foreach (var rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
            rb.simulated = true;

        var stones = Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        foreach (var rb in stones)
        {
            if (rb.gameObject.CompareTag("Stone") && rb.simulated)
            {
                if (!StackManager.Instance.IsInStack(rb.gameObject))
                    Destroy(rb.gameObject);
            }
        }

        StackManager.Instance.ResetStackWidthToOriginal(); 
        DropperController.Instance.SpawnStone();

        uiManager.HideGameOver();
    }

    // need to DELETE AFTER DEVELOPMENT IS DONE
    public void ResetEverythingForTesting()
    {
        Debug.Log("🧹 FULL RESET TRIGGERED");

        // reset gameplay
        level = 1;
        score = 0;
        placedSinceLevel = 0;


        // clear local saves
        PlayerPrefs.DeleteKey("PLAYER_LEVEL");
        PlayerPrefs.DeleteKey("BEST_SCORE");
        PlayerPrefs.Save();

        // reset combo
        ResetCombo();

        // update UI
        uiManager.UpdateLevel(level);
        uiManager.UpdateScore(score);

        // 🔥 CLEAR LEADERBOARD (Firebase)
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.ClearLeaderboardForTesting();
        }

        Debug.Log("✅ Everything reset");
    }

    public float GetLevelProgress01()
    {
        int required = GetBlocksRequiredForLevel(level);
        return (float)placedSinceLevel / required;
    }

    public int GetBlocksPlaced() => placedSinceLevel;
    public int GetBlocksRequired() => GetBlocksRequiredForLevel(level);

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