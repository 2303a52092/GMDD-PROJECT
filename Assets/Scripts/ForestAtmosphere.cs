using UnityEngine;

public sealed class ForestAtmosphere : MonoBehaviour
{
    [SerializeField, Min(30f)] private float daylightCycleSeconds = 240f;

    private GameManager gameManager;
    private Light sun;
    private Color dayFog;
    private Color nightFog;
    private Color dayAmbient;
    private Color nightAmbient;
    private float baseSunIntensity;
    private Material skybox;

    public void Configure(GameManager manager, Light sunLight)
    {
        gameManager = manager;
        sun = sunLight;
        dayFog = new Color(0.30f, 0.36f, 0.36f);
        nightFog = new Color(0.075f, 0.10f, 0.14f);
        dayAmbient = new Color(0.34f, 0.36f, 0.33f);
        nightAmbient = new Color(0.10f, 0.13f, 0.17f);
        baseSunIntensity = sun != null ? sun.intensity : 1f;
        skybox = RenderSettings.skybox;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.007f;
        RenderSettings.ambientLight = dayAmbient;
    }

    private void Update()
    {
        if (gameManager == null)
            return;

        float progress = Mathf.Clamp01(gameManager.RunTime / daylightCycleSeconds);
        float dusk = Mathf.SmoothStep(0f, 1f, progress);
        float night = Mathf.SmoothStep(0.55f, 1f, progress);
        RenderSettings.fogColor = Color.Lerp(dayFog, nightFog, dusk);
        RenderSettings.ambientLight = Color.Lerp(dayAmbient, nightAmbient, dusk);
        RenderSettings.fogDensity = Mathf.Lerp(0.007f, 0.013f, dusk);

        if (skybox != null && skybox.HasProperty("_SkyTint"))
        {
            skybox.SetColor("_SkyTint", Color.Lerp(
                new Color(0.55f, 0.62f, 0.66f), new Color(0.25f, 0.32f, 0.42f), dusk));
            skybox.SetColor("_GroundColor", Color.Lerp(
                new Color(0.19f, 0.22f, 0.21f), new Color(0.06f, 0.08f, 0.11f), dusk));
            skybox.SetFloat("_Exposure", Mathf.Lerp(0.65f, 0.28f, night));
        }

        if (sun != null)
        {
            sun.intensity = Mathf.Lerp(baseSunIntensity, 0.04f, night);
            sun.color = Color.Lerp(new Color(0.82f, 0.86f, 0.82f), new Color(0.38f, 0.48f, 0.68f), dusk);
            sun.transform.rotation = Quaternion.Euler(38f + progress * 62f, -28f + progress * 18f, 0f);
        }
    }
}
