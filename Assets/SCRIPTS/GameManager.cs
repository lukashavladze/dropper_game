using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public DropperController dropper;
    public UIManager uiManager;
    public GameObject perfectPlacementEffect;
    public Transform dropperTransform;

    [Header("Progress")]
    public int score = 0;
    public int level = 1;

    private int placedSinceLevel = 0;
    public bool isGameOver = false;

    [Header("Combo Sounds")]
    public AudioClip[] comboSounds;


    [Header("Audio")]
    public AudioClip dropSound;
    public AudioClip placedSound;
    public AudioClip missSound;
    public AudioClip normalSound;
    private AudioSource audioSource;

    private const string SaveKey_Level = "PLAYER_LEVEL";
    public GameObject scorePopupPrefab;

    [SerializeField] GameObject perfectFlashPrefab;

    private int blocksSinceSpeedIncrease = 0;
    

    // 🔥 COMBO SYSTEM
    private int comboCount = 1;
    private int normalStreak = 0;

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


    void ResetCombo()
    {
        comboCount = 1;
        UIManager.Instance?.UpdateCombo(1, 1);
        BloomController.Instance?.SetTarget(3f, 0.6f);
    }

    Color GetComboColor(int combo)
    {
        // normalize combo (1 → 10)
        float t = Mathf.InverseLerp(1, 10, combo);

        // gradient: blue → cyan → green → yellow → red
        if (t < 0.25f)
            return Color.Lerp(new Color(0.2f, 0.4f, 1f), Color.cyan, t / 0.25f);

        if (t < 0.5f)
            return Color.Lerp(Color.cyan, Color.green, (t - 0.25f) / 0.25f);

        if (t < 0.75f)
            return Color.Lerp(Color.green, Color.yellow, (t - 0.5f) / 0.25f);

        return Color.Lerp(Color.yellow, Color.red, (t - 0.75f) / 0.25f);
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

    void PlayComboSound()
    {
        if (comboSounds == null || comboSounds.Length == 0)
            return;

        // comboCount starts at 1
        int index = Mathf.Clamp(comboCount - 1, 0, comboSounds.Length - 1);

        AudioClip clip = comboSounds[index];

        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void OnMiss(GameObject stone)
    {
        if (isGameOver) return;

        isGameOver = true;
        PlaySound(missSound);

        UIManager.Instance.UpdateScoreGameover(score);

        int best = PlayerPrefs.GetInt("BEST_SCORE", 0);
        if (score > best)
        {
            PlayerPrefs.SetInt("BEST_SCORE", score);
            PlayerPrefs.Save();
            // 🔥 NEW BEST
            uiManager.UpdateBestScoreGameover(score);
        }
        else
        {
            // existing best
            uiManager.UpdateBestScoreGameover(best);
        }

        LeaderboardManager.Instance.SaveScore(score);

        uiManager.ShowGameOver();

        foreach (var rb in Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
            rb.simulated = false;
    }


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
        normalStreak = 0;
        if (placedCount > 1)
        {
            CameraShake.ShakeSafe(0.15f, 0.20f);
        }

        int baseScore = placedCount;
        int multiplier = GetComboMultiplier();
        int finalScore = baseScore * multiplier;

        AddScore(finalScore);
        SpawnScorePopup(stone.transform.position, finalScore); 
        if (placedCount > 1)
        {
            PlaySound(placedSound);
        }
        else if (placedCount == 1)
        {
            PlaySound(normalSound);
        }
        

        MoveDropperUp(stone);

        HandleBlockPlaced();
        StackManager.Instance.WorsenPrecision();
        HandleLevelProgression();
    }


    public void OnPerfectPlacement(int placedCount, GameObject stone)
    {
        PlayComboSound();

        normalStreak = 0;
        // COMBO INCREASE
        if (comboCount <= 9)
            comboCount++;

        //  BLOOM FX
        BloomController.Instance?.SetTarget(10f, 0.7f);
        BloomController.Instance?.Pulse(40f, 0.8f);

        if (comboCount >= 10)
            BloomController.Instance?.SetTarget(10f, 0.7f);
        else
            BloomController.Instance?.SetTarget(6f, 0.5f);

        int multiplier = GetComboMultiplier();
        int baseScore = placedCount;

        int finalScore = baseScore * multiplier; // perfect bonus

        AddScore(finalScore);
        SpawnScorePopup(stone.transform.position, finalScore);

        UIManager.Instance?.UpdateCombo(comboCount, multiplier);

        StartCoroutine(PerfectBounce(stone.transform));
        MoveDropperUp(stone);

        // =========================
        // PERFECT IMPACT FX (COLOR MATCHED)
        // =========================
        if (perfectFlashPrefab != null)
        {
            var sr = stone.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                Bounds b = sr.bounds;
                float halfHeight = sr.bounds.extents.y;

                Vector3 pos = new Vector3(
                stone.transform.position.x,
                stone.transform.position.y - halfHeight - 0.02f,
                stone.transform.position.z - 0.1f
               );

                GameObject fx = Instantiate(perfectFlashPrefab, pos, Quaternion.identity);

                Color color = GetColorFromSprite(sr.sprite);
                color *= 3f;  
                color.a = 1f;

                var systems = fx.GetComponentsInChildren<ParticleSystem>();

                foreach (var ps in systems)
                {
                    var main = ps.main;
                    main.startColor = color;
                }
            }
        }

        // 🔥 GAMEPLAY BONUS
        if (comboCount >= 5)
            StackManager.Instance.ImprovePrecision();

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
        normalStreak++;
        bool comboBreak = normalStreak >= 2;

        // ❗ reset combo if 2 normals in a row
        if (comboBreak)
        {
            ResetCombo();
            PlaySound(placedSound);
            normalStreak = 0;
        }

        int baseScore = placedCount; // 👈 NEW scoring system

        int multiplier = GetComboMultiplier();
        int finalScore = baseScore * multiplier;

        AddScore(finalScore);

        SpawnScorePopup(stone.transform.position, finalScore);
        if (!comboBreak)
        {
            PlaySound(normalSound);
        }

        MoveDropperUp(stone);

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


            PlayerPrefs.Save();


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

    Color GetColorFromSprite(Sprite sprite)
    {
        var skins = DropperController.Instance.stoneSprites;

        if (sprite == skins[0]) return Color.red;
        if (sprite == skins[1]) return Color.blue;
        if (sprite == skins[2]) return Color.yellow;
        if (sprite == skins[3]) return new Color(0.6f, 0f, 1f); // purple
        if (sprite == skins[4]) return Color.green;

        return Color.white;
    }

    // need to DELETE AFTER DEVELOPMENT IS DONE
    public void ResetEverythingForTesting()
    {
        Debug.Log("🧹 FULL RESET TRIGGERED");

        level = 1;
        score = 0;
        placedSinceLevel = 0;

        // clear local saves
        PlayerPrefs.DeleteKey("PLAYER_LEVEL");
        PlayerPrefs.DeleteKey("BEST_SCORE");
        PlayerPrefs.DeleteKey("PLAYER_NAME"); 
        PlayerPrefs.DeleteKey("USER_ID");    
        PlayerPrefs.Save();

        ResetCombo();

        uiManager.UpdateLevel(level);
        uiManager.UpdateScore(score);

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.ClearLeaderboardForTesting();
        }

        var leaderboard = FindFirstObjectByType<LeaderboardUI>();
        if (leaderboard != null)
        {
            leaderboard.Show(); // 🔥 forces username check again
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