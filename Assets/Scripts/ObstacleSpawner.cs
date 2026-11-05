using UnityEngine;

public sealed class ObstacleSpawner : MonoBehaviour
{
    private static readonly float[] LanePositions = { -3f, 0f, 3f };
    private Material obstacleMaterial;
    private Material crateSlatMaterial;
    private Material crateHighlightMaterial;
    private Material coinMaterial;
    private Material healthMaterial;

    public void Populate(Transform parent, int tileIndex, float startZ, float length)
    {
        EnsureMaterials();
        System.Random random = new System.Random(tileIndex * 1543 + 7919);

        for (int obstacleIndex = 0; obstacleIndex < 2; obstacleIndex++)
        {
            int lane = random.Next(0, LanePositions.Length);
            float z = startZ + 13f + obstacleIndex * (length - 27f) + (float)random.NextDouble() * 5f;
            CreateObstacle(parent, lane, z, random.Next(0, 3));

            int coinLane = (lane + random.Next(1, LanePositions.Length)) % LanePositions.Length;
            for (int i = 0; i < 5; i++)
                CreatePickup(parent, "Coin_", PrimitiveType.Cylinder,
                    new Vector3(LanePositions[coinLane], 1.15f, z + i * 2f),
                    new Vector3(0.58f, 0.12f, 0.58f), coinMaterial, true);
        }

        if (random.NextDouble() < 0.16)
        {
            int lane = random.Next(0, LanePositions.Length);
            float z = startZ + length * 0.5f;
            CreatePickup(parent, "HealthKit_", PrimitiveType.Cube,
                new Vector3(LanePositions[lane], 0.8f, z), Vector3.one * 0.75f, healthMaterial, false);
        }
    }

    private void CreateObstacle(Transform parent, int lane, float z, int kind)
    {
        PrimitiveType type = kind == 1 ? PrimitiveType.Sphere : PrimitiveType.Cube;
        GameObject obstacle = GameObject.CreatePrimitive(type);
        obstacle.name = kind == 0 ? "Obstacle_FallenLog" : kind == 1 ? "Obstacle_Rock" : "Obstacle_Crate";
        obstacle.transform.SetParent(parent, true);
        obstacle.transform.position = new Vector3(LanePositions[lane], kind == 0 ? 0.55f : 0.8f, z);

        if (kind == 0)
        {
            obstacle.transform.localScale = new Vector3(0.65f, 3f, 0.65f);
            obstacle.transform.rotation = Quaternion.Euler(90f, 0f, 90f);
        }
        else if (kind == 1)
        {
            obstacle.transform.localScale = new Vector3(1.7f, 1.4f, 1.5f);
        }
        else
        {
            obstacle.transform.localScale = new Vector3(1.9f, 1.55f, 1.35f);
        }

        Renderer renderer = obstacle.GetComponent<Renderer>();
        if (renderer != null && obstacleMaterial != null)
            renderer.sharedMaterial = obstacleMaterial;

        if (kind == 2)
            AddCrateSlats(obstacle.transform);
    }

    private void AddCrateSlats(Transform crate)
    {
        CreateCrateBoard(crate, "Obstacle_CrateSlat", new Vector3(0f, 0f, 0.69f),
            new Vector3(1.7f, 0.12f, 0.045f), crateSlatMaterial);
        CreateCrateBoard(crate, "Obstacle_CrateSlat", new Vector3(0f, 0.63f, 0.69f),
            new Vector3(1.7f, 0.12f, 0.045f), crateSlatMaterial);
        GameObject diagonal = CreateCrateBoard(crate, "Obstacle_CrateSlat", Vector3.zero,
            new Vector3(0.12f, 1.65f, 0.05f), crateHighlightMaterial);
        diagonal.transform.localPosition = new Vector3(0f, 0f, 0.72f);
        diagonal.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
    }

    private static GameObject CreateCrateBoard(Transform parent, string boardName, Vector3 position,
        Vector3 scale, Material material)
    {
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = boardName;
        board.transform.SetParent(parent, false);
        board.transform.localPosition = position;
        board.transform.localScale = scale;
        Collider collider = board.GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;
        Renderer renderer = board.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.sharedMaterial = material;
        return board;
    }

    private static void CreatePickup(Transform parent, string prefix, PrimitiveType type,
        Vector3 position, Vector3 scale, Material material, bool coin)
    {
        GameObject pickup = GameObject.CreatePrimitive(type);
        pickup.name = prefix + System.Guid.NewGuid().ToString("N");
        pickup.transform.SetParent(parent, true);
        pickup.transform.position = position;
        pickup.transform.localScale = scale;
        if (coin)
            pickup.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Collider collider = pickup.GetComponent<Collider>();
        collider.isTrigger = true;
        Rigidbody body = pickup.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        Renderer renderer = pickup.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.sharedMaterial = material;
    }

    private void EnsureMaterials()
    {
        if (obstacleMaterial != null)
            return;
        obstacleMaterial = CreateMaterial(new Color(0.26f, 0.18f, 0.12f));
        crateSlatMaterial = CreateMaterial(new Color(0.34f, 0.22f, 0.12f));
        crateHighlightMaterial = CreateMaterial(new Color(0.43f, 0.29f, 0.16f));
        coinMaterial = CreateMaterial(new Color(0.95f, 0.62f, 0.08f));
        healthMaterial = CreateMaterial(new Color(0.55f, 0.08f, 0.06f));
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
