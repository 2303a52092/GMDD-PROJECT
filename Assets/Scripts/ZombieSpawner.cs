using System.Collections.Generic;
using UnityEngine;

public sealed class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField, Min(1f)] private float spawnInterval = 12f;
    [SerializeField, Range(1, 8)] private int maximumZombies = 4;

    private readonly List<ZombieAI> zombies = new List<ZombieAI>();
    private Transform player;
    private GameManager gameManager;
    private float spawnTimer;
    private static Material zombieSkinMaterial;
    private static Material zombieClothingMaterial;
    private static Material zombieTrouserMaterial;
    private static Material zombieEyeMaterial;

    public float DangerLevel
    {
        get
        {
            if (player == null || zombies.Count == 0)
                return 0f;

            float nearest = float.MaxValue;
            for (int i = 0; i < zombies.Count; i++)
            {
                if (zombies[i] != null)
                    nearest = Mathf.Min(nearest, Vector3.Distance(player.position, zombies[i].transform.position));
            }
            return nearest == float.MaxValue ? 0f : Mathf.Clamp01(1f - (nearest - 3f) / 30f);
        }
    }

    public void Configure(Transform target, GameManager manager, GameObject prefab)
    {
        player = target;
        gameManager = manager;
        zombiePrefab = prefab;
    }

    public void Initialize()
    {
        if (player != null && zombies.Count == 0)
            SpawnZombie();
    }

    private void Update()
    {
        zombies.RemoveAll(zombie => zombie == null);
        if (gameManager == null || gameManager.State != RunState.Playing || zombies.Count >= maximumZombies)
            return;

        spawnTimer += Time.deltaTime;
        float interval = Mathf.Max(5f, spawnInterval / Mathf.Max(1f, gameManager.Difficulty));
        if (spawnTimer >= interval)
        {
            spawnTimer = 0f;
            SpawnZombie();
        }
    }

    private void SpawnZombie()
    {
        Vector3 position = player.position + new Vector3(Random.Range(-1.5f, 1.5f), 0f, -10f - zombies.Count * 2f);
        GameObject zombieObject;

        if (zombiePrefab != null)
        {
            zombieObject = Instantiate(zombiePrefab, position, Quaternion.identity);
            zombieObject.name = "Zombie";
        }
        else
        {
            zombieObject = CreateFallbackZombie(position);
        }

        CharacterController controller = zombieObject.GetComponent<CharacterController>();
        if (controller == null)
            controller = zombieObject.AddComponent<CharacterController>();
        Collider[] colliders = zombieObject.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != controller)
                colliders[i].enabled = false;
        }
        ZombieAI ai = zombieObject.GetComponent<ZombieAI>();
        if (ai == null)
            ai = zombieObject.AddComponent<ZombieAI>();
        ai.Configure(player, gameManager);
        if (zombiePrefab == null)
        {
            Transform visual = zombieObject.transform.Find("ZombieVisual");
            if (visual != null)
            {
                ai.ConfigureProceduralRig(
                    visual.Find("LeftArm"), visual.Find("RightArm"),
                    visual.Find("LeftLeg"), visual.Find("RightLeg"));
            }
        }
        zombies.Add(ai);
    }

    public void ResetRun()
    {
        for (int i = 0; i < zombies.Count; i++)
        {
            if (zombies[i] != null)
                Destroy(zombies[i].gameObject);
        }
        zombies.Clear();
        spawnTimer = 0f;
        Initialize();
    }

    private static GameObject CreateFallbackZombie(Vector3 position)
    {
        EnsureZombieMaterials();
        GameObject root = new GameObject("Zombie");
        root.transform.position = position;
        Transform visual = new GameObject("ZombieVisual").transform;
        visual.SetParent(root.transform, false);

        CreatePart(visual, PrimitiveType.Sphere, "TornCoat",
            new Vector3(0f, 1.03f, 0f), new Vector3(0.68f, 0.86f, 0.45f), zombieClothingMaterial);
        CreatePart(visual, PrimitiveType.Sphere, "DecayedHead",
            new Vector3(0f, 1.78f, 0.08f), new Vector3(0.38f, 0.43f, 0.36f), zombieSkinMaterial);
        CreatePart(visual, PrimitiveType.Cube, "OpenMouth",
            new Vector3(0f, 1.63f, 0.25f), new Vector3(0.12f, 0.09f, 0.035f), zombieEyeMaterial);
        CreatePart(visual, PrimitiveType.Cube, "TornShirt",
            new Vector3(-0.2f, 0.87f, 0.26f), new Vector3(0.18f, 0.25f, 0.03f), zombieSkinMaterial);
        CreatePart(visual, PrimitiveType.Cube, "TornShirt",
            new Vector3(0.21f, 1.12f, 0.24f), new Vector3(0.14f, 0.20f, 0.03f), zombieSkinMaterial);

        Transform leftArm = CreateLimb(visual, "LeftArm", new Vector3(-0.39f, 1.36f, 0.02f),
            new Vector3(0.2f, 0.37f, 0.22f), zombieSkinMaterial);
        Transform rightArm = CreateLimb(visual, "RightArm", new Vector3(0.39f, 1.36f, 0.02f),
            new Vector3(0.2f, 0.37f, 0.22f), zombieSkinMaterial);
        Transform leftLeg = CreateLimb(visual, "LeftLeg", new Vector3(-0.18f, 0.66f, 0f),
            new Vector3(0.23f, 0.38f, 0.25f), zombieTrouserMaterial);
        Transform rightLeg = CreateLimb(visual, "RightLeg", new Vector3(0.18f, 0.66f, 0f),
            new Vector3(0.23f, 0.38f, 0.25f), zombieTrouserMaterial);

        CreatePart(leftArm, PrimitiveType.Cube, "Hand", new Vector3(0f, -0.55f, 0.05f),
            new Vector3(0.22f, 0.17f, 0.20f), zombieSkinMaterial);
        CreatePart(rightArm, PrimitiveType.Cube, "Hand", new Vector3(0f, -0.55f, 0.05f),
            new Vector3(0.22f, 0.17f, 0.20f), zombieSkinMaterial);
        root.AddComponent<CharacterController>();
        return root;
    }

    private static Transform CreateLimb(Transform parent, string partName, Vector3 position,
        Vector3 scale, Material material)
    {
        Transform pivot = new GameObject(partName).transform;
        pivot.SetParent(parent, false);
        pivot.localPosition = position;
        CreatePart(pivot, PrimitiveType.Sphere, partName + "Mesh", new Vector3(0f, -0.27f, 0f), scale, material);
        return pivot;
    }

    private static GameObject CreatePart(Transform parent, PrimitiveType type, string partName,
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
        if (renderer != null)
            renderer.sharedMaterial = material;
        return part;
    }

    private static void EnsureZombieMaterials()
    {
        if (zombieSkinMaterial != null)
            return;
        zombieSkinMaterial = CreateMaterial(new Color(0.36f, 0.40f, 0.31f));
        zombieClothingMaterial = CreateMaterial(new Color(0.18f, 0.16f, 0.14f));
        zombieTrouserMaterial = CreateMaterial(new Color(0.13f, 0.15f, 0.14f));
        zombieEyeMaterial = CreateMaterial(new Color(0.36f, 0.05f, 0.035f));
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
