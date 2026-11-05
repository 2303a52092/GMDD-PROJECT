using UnityEngine;

public sealed class UIManager : MonoBehaviour
{
    private GameManager gameManager;
    private HealthSystem health;
    private PlayerController player;
    private ZombieSpawner zombies;
    private AudioManager audioManager;
    private GUIStyle titleStyle;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private GUIStyle iconStyle;
    private GUIStyle pauseStyle;
    private GUIStyle buttonStyle;
    private Texture2D panelTexture;
    private Texture2D pauseTexture;
    private Texture2D barTexture;
    private bool settingsOpen;
    private float masterVolume = 0.8f;

    public void Configure(GameManager manager, HealthSystem healthSystem,
        PlayerController playerController, ZombieSpawner zombieSpawner, AudioManager audio)
    {
        gameManager = manager;
        health = healthSystem;
        player = playerController;
        zombies = zombieSpawner;
        audioManager = audio;
        AudioListener.volume = masterVolume;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (settingsOpen)
        {
            settingsOpen = false;
        }
        else if (gameManager != null && gameManager.State == RunState.Playing)
        {
            gameManager.Pause();
        }
        else if (gameManager != null && gameManager.State == RunState.Paused)
        {
            gameManager.Resume();
        }
    }

    private void OnGUI()
    {
        if (gameManager == null)
            return;

        EnsureStyles();
        float scale = Mathf.Clamp(Screen.height / 576f, 0.7f, 1.5f);

        if (settingsOpen)
        {
            DrawSettings(scale);
            return;
        }

        if (gameManager.State == RunState.MainMenu)
        {
            DrawMenu(scale);
            return;
        }

        if (gameManager.State == RunState.Playing || gameManager.State == RunState.Paused)
        {
            DrawHud(scale);
            DrawPauseButton(scale);
        }

        if (gameManager.State == RunState.Paused)
            DrawPauseMenu(scale);
        else if (gameManager.State == RunState.GameOver)
            DrawGameOver(scale);
    }

    private void DrawHud(float scale)
    {
        float x = 20f * scale;
        float y = 16f * scale;
        float distanceWidth = 205f * scale;
        float coinX = x + distanceWidth + 10f * scale;
        float coinWidth = 150f * scale;
        float rowHeight = 42f * scale;
        float healthY = y + rowHeight + 7f * scale;

        DrawCard(new Rect(x, y, distanceWidth, rowHeight));
        DrawIcon("➤", new Rect(x + 10f * scale, y, 35f * scale, rowHeight), new Color(1f, 0.68f, 0.08f));
        GUI.Label(new Rect(x + 49f * scale, y, distanceWidth - 56f * scale, rowHeight),
            "DISTANCE  " + Mathf.FloorToInt(gameManager.Distance) + " m", valueStyle);

        DrawCard(new Rect(coinX, y, coinWidth, rowHeight));
        DrawIcon("★", new Rect(coinX + 8f * scale, y, 34f * scale, rowHeight), new Color(1f, 0.71f, 0.06f));
        GUI.Label(new Rect(coinX + 45f * scale, y, coinWidth - 50f * scale, rowHeight),
            "COINS  " + gameManager.Coins, valueStyle);

        DrawCard(new Rect(x, healthY, distanceWidth, 47f * scale));
        int currentHealth = health != null ? health.CurrentHealth : 0;
        int maxHealth = health != null ? health.MaximumHealth : 1;
        DrawIcon("♥", new Rect(x + 8f * scale, healthY, 37f * scale, 39f * scale), new Color(0.94f, 0.12f, 0.12f));
        GUI.Label(new Rect(x + 49f * scale, healthY + 1f * scale, 125f * scale, 23f * scale),
            "HEALTH : " + currentHealth, valueStyle);
        DrawMeter(new Rect(x + 49f * scale, healthY + 27f * scale, 146f * scale, 9f * scale),
            (float)currentHealth / maxHealth, new Color(0.91f, 0.14f, 0.12f));

        float danger = zombies != null ? zombies.DangerLevel : 0f;
        float stamina = player != null ? player.Stamina : 1f;
        float statusX = x;
        float statusY = healthY + 56f * scale;
        float statusWidth = distanceWidth;
        DrawCard(new Rect(statusX, statusY, statusWidth, 54f * scale));
        GUI.Label(new Rect(statusX + 9f * scale, statusY + 1f * scale, statusWidth - 18f * scale, 20f * scale),
            "ZOMBIE THREAT", valueStyle);
        DrawMeter(new Rect(statusX + 9f * scale, statusY + 23f * scale,
                statusWidth - 18f * scale, 7f * scale),
            danger, Color.Lerp(new Color(0.18f, 0.59f, 0.24f), new Color(0.91f, 0.10f, 0.08f), danger));

        if (Screen.width >= 850f)
        {
            float rightX = Mathf.Max(coinX + coinWidth + 18f * scale, Screen.width - 400f * scale);
            DrawCard(new Rect(rightX, y, 172f * scale, rowHeight));
            GUI.Label(new Rect(rightX + 8f * scale, y, 156f * scale, rowHeight),
                "SCORE  " + gameManager.Score, valueStyle);
            DrawCard(new Rect(rightX, y + rowHeight + 7f * scale, 172f * scale, rowHeight));
            GUI.Label(new Rect(rightX + 8f * scale, y + rowHeight + 7f * scale, 156f * scale, rowHeight),
                "TIME  " + FormatTime(gameManager.RunTime), valueStyle);
            DrawCard(new Rect(rightX, y + (rowHeight + 7f * scale) * 2f, 172f * scale, 47f * scale));
            GUI.Label(new Rect(rightX + 8f * scale, y + (rowHeight + 7f * scale) * 2f, 156f * scale, 22f * scale),
                player != null && player.IsSprinting ? "SPRINT" : "STAMINA", valueStyle);
            DrawMeter(new Rect(rightX + 8f * scale, y + (rowHeight + 7f * scale) * 2f + 27f * scale,
                156f * scale, 8f * scale), stamina, new Color(0.82f, 0.66f, 0.24f));
        }
    }

    private void DrawMeter(Rect rect, float amount, Color fill)
    {
        GUI.DrawTexture(rect, barTexture);
        Color oldColor = GUI.color;
        GUI.color = fill;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(amount), rect.height), barTexture);
        GUI.color = oldColor;
    }

    private void DrawPauseButton(float scale)
    {
        float diameter = 54f * scale;
        Rect button = new Rect(Screen.width - diameter - 20f * scale, 12f * scale, diameter, diameter);
        if (GUI.Button(button, "Ⅱ", pauseStyle))
            gameManager.Pause();
    }

    private void DrawMenu(float scale)
    {
        Rect panel = PanelRect(scale, 320f);
        DrawPanelBackground(panel);
        GUI.Label(new Rect(panel.x + 12f * scale, panel.y + 35f * scale,
            panel.width - 24f * scale, 58f * scale), "DEEP FOREST", titleStyle);
        GUI.Label(new Rect(panel.x + 12f * scale, panel.y + 91f * scale,
            panel.width - 24f * scale, 32f * scale), "ZOMBIE APOCALYPSE", labelStyle);
        GUI.Label(new Rect(panel.x + 16f * scale, panel.y + 139f * scale,
            panel.width - 32f * scale, 42f * scale),
            "A survivor. An endless forest. Don't let them catch you.", labelStyle);
        if (GUI.Button(new Rect(panel.x + panel.width * 0.24f, panel.y + 194f * scale,
                panel.width * 0.52f, 46f * scale), "START RUN", buttonStyle))
            gameManager.StartRun();
        if (GUI.Button(new Rect(panel.x + panel.width * 0.24f, panel.y + 249f * scale,
                panel.width * 0.52f, 42f * scale), "SETTINGS", buttonStyle))
            settingsOpen = true;
    }

    private void DrawPauseMenu(float scale)
    {
        Rect panel = PanelRect(scale, 320f);
        DrawPanelBackground(panel);
        GUI.Label(new Rect(panel.x + 12f * scale, panel.y + 24f * scale,
            panel.width - 24f * scale, 52f * scale), "PAUSED", titleStyle);
        DrawMenuButton(panel, "RESUME", 104f, scale, gameManager.Resume);
        DrawMenuButton(panel, "RESTART RUN", 160f, scale, gameManager.RestartRun);
        if (GUI.Button(new Rect(panel.x + panel.width * 0.25f, panel.y + 216f * scale,
                panel.width * 0.5f, 42f * scale), "SETTINGS", buttonStyle))
            settingsOpen = true;
        if (GUI.Button(new Rect(panel.x + panel.width * 0.25f, panel.y + 266f * scale,
                panel.width * 0.5f, 38f * scale), "MAIN MENU", buttonStyle))
            gameManager.ReturnToMenu();
    }

    private void DrawGameOver(float scale)
    {
        Rect panel = PanelRect(scale, 320f);
        DrawPanelBackground(panel);
        GUI.Label(new Rect(panel.x + 12f * scale, panel.y + 28f * scale,
            panel.width - 24f * scale, 52f * scale), "YOU WERE CAUGHT", titleStyle);
        GUI.Label(new Rect(panel.x + 12f * scale, panel.y + 91f * scale,
            panel.width - 24f * scale, 30f * scale),
            "Distance  " + Mathf.FloorToInt(gameManager.Distance) + " m", labelStyle);
        GUI.Label(new Rect(panel.x + 12f * scale, panel.y + 124f * scale,
            panel.width - 24f * scale, 30f * scale),
            "Score  " + gameManager.Score + "     Coins  " + gameManager.Coins, labelStyle);
        DrawMenuButton(panel, "RUN AGAIN", 181f, scale, gameManager.RestartRun);
        DrawMenuButton(panel, "MAIN MENU", 238f, scale, gameManager.ReturnToMenu);
    }

    private void DrawSettings(float scale)
    {
        Rect panel = PanelRect(scale, 300f);
        DrawPanelBackground(panel);
        GUI.Label(new Rect(panel.x + 12f * scale, panel.y + 28f * scale,
            panel.width - 24f * scale, 54f * scale), "SETTINGS", titleStyle);
        GUI.Label(new Rect(panel.x + 30f * scale, panel.y + 110f * scale,
            135f * scale, 34f * scale), "MASTER VOLUME", valueStyle);
        float sliderX = panel.x + 175f * scale;
        masterVolume = GUI.HorizontalSlider(
            new Rect(sliderX, panel.y + 118f * scale, panel.width - 210f * scale, 24f * scale),
            masterVolume, 0f, 1f);
        AudioListener.volume = masterVolume;
        GUI.Label(new Rect(panel.x + 30f * scale, panel.y + 158f * scale,
            panel.width - 60f * scale, 30f * scale),
            "QUALITY  " + QualitySettings.names[QualitySettings.GetQualityLevel()], labelStyle);
        if (GUI.Button(new Rect(panel.x + panel.width * 0.3f, panel.y + 195f * scale,
                panel.width * 0.4f, 40f * scale), "CYCLE QUALITY", buttonStyle))
        {
            int qualityCount = QualitySettings.names.Length;
            if (qualityCount > 0)
                QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel() + 1) % qualityCount, true);
        }
        if (GUI.Button(new Rect(panel.x + panel.width * 0.3f, panel.y + 248f * scale,
                panel.width * 0.4f, 42f * scale), "BACK", buttonStyle))
            settingsOpen = false;
    }

    private void DrawMenuButton(Rect panel, string text, float y, float scale, UnityEngine.Events.UnityAction action)
    {
        if (GUI.Button(new Rect(panel.x + panel.width * 0.25f, panel.y + y * scale,
                panel.width * 0.5f, 44f * scale), text, buttonStyle))
            action.Invoke();
    }

    private Rect PanelRect(float scale, float height)
    {
        float width = Mathf.Min(500f * scale, Screen.width - 32f);
        float scaledHeight = height * scale;
        return new Rect((Screen.width - width) * 0.5f,
            (Screen.height - scaledHeight) * 0.5f, width, scaledHeight);
    }

    private void DrawPanelBackground(Rect panel)
    {
        GUI.DrawTexture(panel, panelTexture);
    }

    private void DrawCard(Rect rect)
    {
        GUI.DrawTexture(rect, panelTexture);
        Color oldColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.12f);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), barTexture);
        GUI.color = oldColor;
    }

    private void DrawIcon(string text, Rect rect, Color color)
    {
        Color oldColor = GUI.color;
        GUI.color = color;
        GUI.Label(rect, text, iconStyle);
        GUI.color = oldColor;
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
            return;

        titleStyle = CreateStyle(30, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        labelStyle = CreateStyle(18, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.88f, 0.90f, 0.86f));
        valueStyle = CreateStyle(16, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        iconStyle = CreateStyle(26, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        pauseStyle = CreateStyle(25, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        pauseTexture = CreateCircleTexture();
        pauseStyle.normal.background = pauseTexture;
        pauseStyle.hover.background = pauseTexture;
        pauseStyle.active.background = pauseTexture;
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            hover = { textColor = Color.white },
            active = { textColor = Color.white }
        };
        panelTexture = CreateTexture(new Color(0.025f, 0.035f, 0.03f, 0.86f));
        barTexture = CreateTexture(Color.white);
    }

    private static GUIStyle CreateStyle(int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color)
    {
        return new GUIStyle(GUI.skin.label)
        {
            alignment = alignment,
            fontSize = fontSize,
            fontStyle = fontStyle,
            normal = { textColor = color }
        };
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        return (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
    }

    private static Texture2D CreateTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateCircleTexture()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f));
                float alpha = Mathf.Clamp01((size * 0.47f - distance) * 0.7f);
                texture.SetPixel(x, y, new Color(0.025f, 0.035f, 0.03f, alpha * 0.92f));
            }
        }
        texture.Apply();
        return texture;
    }

    private void OnDestroy()
    {
        AudioListener.volume = 1f;
        if (panelTexture != null)
            Destroy(panelTexture);
        if (pauseTexture != null)
            Destroy(pauseTexture);
        if (barTexture != null)
            Destroy(barTexture);
    }
}
