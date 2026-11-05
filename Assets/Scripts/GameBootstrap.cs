using UnityEngine;

public sealed class GameBootstrap : MonoBehaviour
{
    [Header("Optional visual assets (all gameplay works without these)")]
    public GameObject survivorVisualPrefab;
    public GameObject zombiePrefab;
    public GameObject treePrefab;
    public GameObject bushPrefab;

    [Header("Optional audio clips")]
    public AudioClip forestAmbience;
    public AudioClip horrorMusic;
    public AudioClip playerFootsteps;
    public AudioClip collisionSound;
    public AudioClip zombieGrowl;

    private bool built;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBootstrap()
    {
        if (FindAnyObjectByType<GameBootstrap>() == null)
            new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
    }

    private void Start()
    {
        if (built)
            return;
        built = true;
        ConfigureEnvironment();
        BuildGame();
    }

    private void BuildGame()
    {
        GameObject systems = new GameObject("GameSystems");
        GameManager gameManager = systems.AddComponent<GameManager>();
        CoinManager coinManager = systems.AddComponent<CoinManager>();
        AudioManager audioManager = systems.AddComponent<AudioManager>();
        audioManager.Configure(forestAmbience, horrorMusic, playerFootsteps, collisionSound, zombieGrowl);
        UIManager uiManager = systems.AddComponent<UIManager>();

        GameObject playerObject = new GameObject("Player");
        playerObject.transform.position = Vector3.zero;
        CharacterController controller = playerObject.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.38f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        HealthSystem health = playerObject.AddComponent<HealthSystem>();
        PlayerController player = playerObject.AddComponent<PlayerController>();
        player.Configure(gameManager, health);
        ConfigurePlayerVisual(player, playerObject.transform);

        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }
        camera.fieldOfView = 65f;
        CameraFollow cameraFollow = camera.GetComponent<CameraFollow>();
        if (cameraFollow == null)
            cameraFollow = camera.gameObject.AddComponent<CameraFollow>();
        cameraFollow.Configure(playerObject.transform);
        camera.transform.position = playerObject.transform.position + new Vector3(0f, 4.5f, -8f);

        GameObject environment = new GameObject("ProceduralEnvironment");
        ForestGenerator forest = environment.AddComponent<ForestGenerator>();
        forest.Configure(treePrefab, bushPrefab);
        ObstacleSpawner obstacles = environment.AddComponent<ObstacleSpawner>();
        GroundSpawner ground = environment.AddComponent<GroundSpawner>();
        ground.Configure(playerObject.transform, forest, obstacles);

        ZombieSpawner zombies = systems.AddComponent<ZombieSpawner>();
        zombies.Configure(playerObject.transform, gameManager, zombiePrefab);
        uiManager.Configure(gameManager, health, player, zombies, audioManager);
        gameManager.Configure(player, health, ground, zombies, coinManager, audioManager);

        ForestAtmosphere atmosphere = environment.AddComponent<ForestAtmosphere>();
        atmosphere.Configure(gameManager, FindAnyObjectByType<Light>());
        ground.Initialize();
        zombies.Initialize();
    }

    private void ConfigurePlayerVisual(PlayerController player, Transform parent)
    {
        if (survivorVisualPrefab != null)
        {
            GameObject prefabVisual = Instantiate(survivorVisualPrefab, parent);
            prefabVisual.name = "SurvivorVisual";
            prefabVisual.transform.localPosition = Vector3.zero;
            prefabVisual.transform.localRotation = Quaternion.identity;
            Collider[] colliders = prefabVisual.GetComponentsInChildren<Collider>();
            for (int i = 0; i < colliders.Length; i++)
                colliders[i].enabled = false;
            player.SendMessage("CacheAnimator", SendMessageOptions.DontRequireReceiver);
            return;
        }

        Transform visual = new GameObject("SurvivorVisual").transform;
        visual.SetParent(parent, false);
        Material jacket = CreateMaterial(new Color(0.27f, 0.34f, 0.27f));
        Material pants = CreateMaterial(new Color(0.13f, 0.17f, 0.15f));
        Material skin = CreateMaterial(new Color(0.68f, 0.48f, 0.34f));
        Material pack = CreateMaterial(new Color(0.30f, 0.19f, 0.11f));
        Material boots = CreateMaterial(new Color(0.09f, 0.08f, 0.07f));
        Material visor = CreateMaterial(new Color(0.10f, 0.15f, 0.15f));

        CreateVisualPart(visual, PrimitiveType.Sphere, "Jacket", new Vector3(0f, 1.05f, 0f),
            new Vector3(0.65f, 0.87f, 0.45f), jacket);
        CreateVisualPart(visual, PrimitiveType.Sphere, "Head", new Vector3(0f, 1.74f, 0.015f),
            new Vector3(0.39f, 0.43f, 0.37f), skin);
        CreateVisualPart(visual, PrimitiveType.Sphere, "Hood", new Vector3(0f, 1.82f, -0.07f),
            new Vector3(0.43f, 0.45f, 0.40f), jacket);
        CreateVisualPart(visual, PrimitiveType.Sphere, "Hair", new Vector3(0f, 2.00f, 0.01f),
            new Vector3(0.30f, 0.16f, 0.30f), boots);
        CreateVisualPart(visual, PrimitiveType.Cube, "FaceCover", new Vector3(0f, 1.68f, 0.19f),
            new Vector3(0.28f, 0.12f, 0.07f), visor);
        CreateVisualPart(visual, PrimitiveType.Cube, "Backpack", new Vector3(0f, 1.12f, -0.34f),
            new Vector3(0.48f, 0.57f, 0.28f), pack);
        CreateVisualPart(visual, PrimitiveType.Cube, "PackFlap", new Vector3(0f, 1.35f, -0.35f),
            new Vector3(0.39f, 0.13f, 0.30f), jacket);

        Transform leftArm = CreateLimb(visual, "LeftArm", new Vector3(-0.37f, 1.32f, 0f), jacket);
        Transform rightArm = CreateLimb(visual, "RightArm", new Vector3(0.37f, 1.32f, 0f), jacket);
        Transform leftLeg = CreateLimb(visual, "LeftLeg", new Vector3(-0.17f, 0.68f, 0f), pants);
        Transform rightLeg = CreateLimb(visual, "RightLeg", new Vector3(0.17f, 0.68f, 0f), pants);
        CreateVisualPart(leftLeg, PrimitiveType.Cube, "Boot", new Vector3(0f, -0.51f, 0.08f),
            new Vector3(0.23f, 0.15f, 0.36f), boots);
        CreateVisualPart(rightLeg, PrimitiveType.Cube, "Boot", new Vector3(0f, -0.51f, 0.08f),
            new Vector3(0.23f, 0.15f, 0.36f), boots);
        player.ConfigureProceduralVisual(visual, leftArm, rightArm, leftLeg, rightLeg);
    }

    private static Transform CreateLimb(Transform parent, string partName, Vector3 position, Material material)
    {
        Transform pivot = new GameObject(partName).transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = position;
        Vector3 limbScale = partName.Contains("Arm")
            ? new Vector3(0.24f, 0.50f, 0.25f)
            : new Vector3(0.30f, 0.58f, 0.30f);
        CreateVisualPart(pivot, PrimitiveType.Sphere, partName + "Mesh", new Vector3(0f, -0.31f, 0f),
            limbScale, material);
        return pivot;
    }

    private static GameObject CreateVisualPart(Transform parent, PrimitiveType type, string partName,
        Vector3 position, Vector3 scale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.sharedMaterial = material;
        return part;
    }

    private static void ConfigureEnvironment()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.30f, 0.36f, 0.36f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.007f;
        RenderSettings.ambientLight = new Color(0.34f, 0.36f, 0.33f);
        QualitySettings.shadowDistance = 120f;
        ConfigureSkybox();

        Light sun = FindAnyObjectByType<Light>();
        if (sun == null)
        {
            GameObject lightObject = new GameObject("ForestSun");
            sun = lightObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 0.95f;
            lightObject.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
        }
        sun.shadows = LightShadows.Soft;
        RenderSettings.sun = sun;
    }

    private static void ConfigureSkybox()
    {
        Shader shader = Shader.Find("Skybox/Procedural");
        if (shader == null)
            return;

        Material skybox = new Material(shader);
        skybox.name = "DeepForest_OvercastSky";
        skybox.SetColor("_SkyTint", new Color(0.55f, 0.62f, 0.66f));
        skybox.SetColor("_GroundColor", new Color(0.19f, 0.22f, 0.21f));
        skybox.SetFloat("_Exposure", 0.65f);
        skybox.SetFloat("_AtmosphereThickness", 1.25f);
        skybox.SetFloat("_SunSize", 0.012f);
        RenderSettings.skybox = skybox;
    }

    private static Material CreateMaterial(Color color)
    {
        bool usingScriptablePipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
        Shader shader = Shader.Find(usingScriptablePipeline ? "Universal Render Pipeline/Lit" : "Standard");
        if (shader == null)
            shader = Shader.Find(usingScriptablePipeline ? "Standard" : "Universal Render Pipeline/Lit");
        if (shader == null)
            return null;
        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}
