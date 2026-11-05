using UnityEngine;

public enum RunState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public sealed class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public RunState State { get; private set; } = RunState.MainMenu;
    public float Distance { get; private set; }
    public int Coins { get; private set; }
    public float RunTime { get; private set; }
    public int Score => Mathf.FloorToInt(Distance) + Coins * 25;
    public float Difficulty => 1f + Distance / 300f;

    private PlayerController player;
    private HealthSystem health;
    private GroundSpawner groundSpawner;
    private ZombieSpawner zombieSpawner;
    private CoinManager coinManager;
    private AudioManager audioManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        State = RunState.MainMenu;
    }

    public void Configure(PlayerController playerController, HealthSystem healthSystem,
        GroundSpawner ground, ZombieSpawner zombies, CoinManager coins, AudioManager audio)
    {
        player = playerController;
        health = healthSystem;
        groundSpawner = ground;
        zombieSpawner = zombies;
        coinManager = coins;
        audioManager = audio;
    }

    private void Update()
    {
        if (player != null && State == RunState.Playing)
        {
            Distance = Mathf.Max(Distance, player.transform.position.z);
            RunTime += Time.deltaTime;
        }

    }

    public void StartRun()
    {
        Time.timeScale = 1f;
        State = RunState.Playing;
        if (audioManager != null)
            audioManager.PlayRunAudio();
    }

    public void Pause()
    {
        if (State != RunState.Playing)
            return;

        State = RunState.Paused;
        Time.timeScale = 0f;
        if (audioManager != null)
            audioManager.StopFootsteps();
    }

    public void Resume()
    {
        if (State != RunState.Paused)
            return;

        Time.timeScale = 1f;
        State = RunState.Playing;
    }

    public void RestartRun()
    {
        Time.timeScale = 1f;
        Distance = 0f;
        Coins = 0;
        RunTime = 0f;

        if (player != null)
            player.ResetRun();
        if (health != null)
            health.ResetHealth();
        if (coinManager != null)
            coinManager.ResetRun();
        if (groundSpawner != null)
            groundSpawner.ResetWorld();
        if (zombieSpawner != null)
            zombieSpawner.ResetRun();

        State = RunState.Playing;
        if (audioManager != null)
            audioManager.PlayRunAudio();
    }

    public void AddCoins(int amount)
    {
        Coins += Mathf.Max(0, amount);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        Distance = 0f;
        Coins = 0;
        RunTime = 0f;
        if (player != null)
            player.ResetRun();
        if (health != null)
            health.ResetHealth();
        if (coinManager != null)
            coinManager.ResetRun();
        if (groundSpawner != null)
            groundSpawner.ResetWorld();
        if (zombieSpawner != null)
            zombieSpawner.ResetRun();
        State = RunState.MainMenu;
        if (audioManager != null)
            audioManager.StopFootsteps();
    }

    public void OnPlayerHealthChanged(int currentHealth)
    {
        if (currentHealth <= 0 && State != RunState.GameOver)
        {
            State = RunState.GameOver;
            Time.timeScale = 1f;
            if (audioManager != null)
                audioManager.StopFootsteps();
        }
    }

    public void PlayCollisionSound()
    {
        if (audioManager != null)
            audioManager.PlayCollision();
    }

    public void PlayZombieSound()
    {
        if (audioManager != null)
            audioManager.PlayZombie();
    }

    public void SetPlayerFootsteps(bool playing)
    {
        if (audioManager != null)
            audioManager.SetFootsteps(playing);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Time.timeScale = 1f;
        }
    }
}
