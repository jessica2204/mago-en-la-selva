using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Central game manager (Singleton) for Mario en la Selva top-down.
/// Controls: score, lives, timer, HUD updates, audio, difficulty scaling,
/// game over and level complete states.
/// Place on an empty GameObject called "GameManager".
/// </summary>
public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ── HUD ──────────────────────────────────────────────────────────────
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI difficultyText; // shows current wave

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject startPanel;         // shown before game starts
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText; // shown on Game Over panel
    [SerializeField] private TextMeshProUGUI finalTimeText;

    // ── Game Settings ─────────────────────────────────────────────────────
    [Header("Game Settings")]
    [SerializeField] private int   startingLives     = 3;
    [SerializeField] private float difficultyInterval = 15f; // seconds between difficulty bumps

    // ── Audio ─────────────────────────────────────────────────────────────
    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip   coinClip;
    [SerializeField] private AudioClip   hitClip;
    [SerializeField] private AudioClip   gameOverClip;

    // ── State ─────────────────────────────────────────────────────────────
    private int   currentScore;
    private int   currentLives;
    private float elapsedTime;
    private int   difficultyLevel;
    private bool  gameRunning = false;

    private PlayerController player;
    private EnemySpawner     spawner;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        player  = FindObjectOfType<PlayerController>();
        spawner = FindObjectOfType<EnemySpawner>();

        // Show start panel, freeze time until player presses Start
        Time.timeScale = 0f;
        if (startPanel    != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        ResetValues();
        UpdateHUD();
    }

    private void Update()
    {
        if (!gameRunning) return;

        elapsedTime += Time.deltaTime;

        // Update timer display
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        if (timeText != null)
            timeText.text = "Time: " + minutes.ToString("00") + ":" + seconds.ToString("00");

        // Increase difficulty every interval
        if (Mathf.FloorToInt(elapsedTime / difficultyInterval) > difficultyLevel)
        {
            difficultyLevel++;
            OnDifficultyIncrease();
        }
    }

    // ── Public: called by UI Start Button ────────────────────────────────

    /// <summary>Called when the player presses the Start button.</summary>
    public void StartGame()
    {
        if (startPanel != null) startPanel.SetActive(false);
        Time.timeScale = 1f;
        gameRunning    = true;
        if (musicSource != null) musicSource.Play();
    }

    // ── Score ─────────────────────────────────────────────────────────────

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateHUD();

        // Check win condition
        if (currentScore >= 100 && gameRunning)
        {
            TriggerVictory();
        }
    }
   
    public void TriggerVictory()
    {
        gameRunning = false;
        victoryPanel.SetActive(true);

        // Hide HUD
        scoreText.gameObject.SetActive(false);
        livesText.gameObject.SetActive(false);
        timeText.gameObject.SetActive(false);
        difficultyText.gameObject.SetActive(false);
    }

    // ── Lives ─────────────────────────────────────────────────────────────


    public void LoseLife()
    {
        if (!gameRunning) return;
        currentLives--;
        PlayHitSound();
        UpdateHUD();

        if (currentLives <= 0)
            TriggerGameOver();
    }

    public void AddLife()
    {
        if (!gameRunning) return;
        if (currentLives < startingLives)
        {
            currentLives++;
            UpdateHUD();
        }
    }


    // ── Difficulty ────────────────────────────────────────────────────────

    /// <summary>Called every difficultyInterval seconds to ramp up the game.</summary>
    private void OnDifficultyIncrease()
    {
        if (difficultyText != null)
            difficultyText.text = "Wave " + (difficultyLevel + 1);

        // Speed up the player slightly so it stays fair
        if (player != null)
            player.SetMoveSpeed(player.GetMoveSpeed() + 0.3f);
    }

    // ── Game Over ─────────────────────────────────────────────────────────

    private void TriggerGameOver()
    {
        gameRunning = false;

        if (player  != null) player.SetAlive(false);
        if (spawner != null) spawner.StopSpawning();
        if (musicSource != null) musicSource.Stop();

        PlayGameOverSound();

        // Show final stats
        int m = Mathf.FloorToInt(elapsedTime / 60f);
        int s = Mathf.FloorToInt(elapsedTime % 60f);
        if (finalScoreText != null) finalScoreText.text = "Score: " + currentScore;
        if (finalTimeText  != null) finalTimeText.text  = "Time: " + m.ToString("00") + ":" + s.ToString("00");
        if (gameOverPanel  != null) gameOverPanel.SetActive(true);
    }

    /// <summary>Restart button on Game Over panel.</summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ── HUD ───────────────────────────────────────────────────────────────

    private void UpdateHUD()
    {
        if (scoreText != null) scoreText.text = "Score: " + currentScore;
        if (livesText != null) livesText.text = "Lives: " + BuildLivesIcons(currentLives);
    }

    /// <summary>Shows lives as heart icons (♥) for visual appeal.</summary>
    private string BuildLivesIcons(int lives)
    {
        string icons = "";
        for (int i = 0; i < Mathf.Max(lives, 0); i++) icons += "♥ ";
        return icons;
    }

    private void ResetValues()
    {
        currentScore    = 0;
        currentLives    = startingLives;
        elapsedTime     = 0f;
        difficultyLevel = 0;
    }

    // ── Audio helpers ─────────────────────────────────────────────────────

    public void PlayCoinSound()
    {
        if (sfxSource != null && coinClip != null)
            sfxSource.PlayOneShot(coinClip);
    }

    public void PlayHitSound()
    {
        if (sfxSource != null && hitClip != null)
            sfxSource.PlayOneShot(hitClip);
    }

    private void PlayGameOverSound()
    {
        if (sfxSource != null && gameOverClip != null)
            sfxSource.PlayOneShot(gameOverClip);
    }
}
Add victory condition and game over HUD fix
