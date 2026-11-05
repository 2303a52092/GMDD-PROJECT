using UnityEngine;

public sealed class ForestGenerator : MonoBehaviour
{
    private static Mesh coniferMesh;

    [SerializeField] private GameObject treePrefab;
    [SerializeField] private GameObject bushPrefab;
    [SerializeField, Min(1)] private int treesPerSide = 22;
    [SerializeField, Min(0)] private int bushesPerTile = 20;

    private Material trunkMaterial;
    private Material[] coniferMaterials;
    private Material[] canopyMaterials;
    private Material[] bushMaterials;
    private Material groundMaterial;
    private Material rockMaterial;
    private Material waterMaterial;
    private Material flowerMaterial;

    public void Configure(GameObject tree, GameObject bush)
    {
        treePrefab = tree;
        bushPrefab = bush;
    }

    public void Populate(Transform parent, int tileIndex, float startZ, float length)
    {
        System.Random random = new System.Random(tileIndex * 7919 + 104729);
        EnsureMaterials();

        CreatePrimitive(parent, PrimitiveType.Cube, "Forest_MudPath",
            new Vector3(0f, -0.035f, startZ + length * 0.5f),
            new Vector3(9.5f, 0.05f, length), groundMaterial, false);
        const float streamSide = 1f;
        CreatePrimitive(parent, PrimitiveType.Cube, "Forest_Stream",
            new Vector3(streamSide * 30f, 0.015f, startZ + length * 0.5f),
            new Vector3(2.5f, 0.035f, length), waterMaterial, false);
        CreateBridge(parent, streamSide, startZ + length * 0.5f);

        for (int i = 0; i < treesPerSide; i++)
        {
            float z = startZ + (float)random.NextDouble() * length;
            SpawnTree(parent, random, -1, z);
            SpawnTree(parent, random, 1, z + 1.7f);
        }

        for (int i = 0; i < bushesPerTile; i++)
        {
            float side = random.Next(0, 2) == 0 ? -1f : 1f;
            float x = side * (8f + (float)random.NextDouble() * 38f);
            float z = startZ + (float)random.NextDouble() * length;
            if (bushPrefab != null)
            {
                GameObject bush = Instantiate(bushPrefab, new Vector3(x, 0f, z),
                    Quaternion.Euler(0f, random.Next(0, 360), 0f), parent);
                bush.name = "Forest_Bush";
            }
            else
            {
                float size = 0.6f + (float)random.NextDouble() * 1.3f;
                SpawnBush(parent, random, x, z, size);
            }
        }

        for (int i = 0; i < 12; i++)
        {
            float side = random.Next(0, 2) == 0 ? -1f : 1f;
            float x = side * (6f + (float)random.NextDouble() * 38f);
            float z = startZ + (float)random.NextDouble() * length;
            float height = 0.14f + (float)random.NextDouble() * 0.28f;
            CreatePrimitive(parent, PrimitiveType.Cylinder, "Forest_WildflowerStem",
                new Vector3(x, height * 0.5f, z), new Vector3(0.025f, height * 0.5f, 0.025f),
                bushMaterials[random.Next(bushMaterials.Length)], false);
            CreatePrimitive(parent, PrimitiveType.Sphere, "Forest_Wildflower",
                new Vector3(x, height, z), new Vector3(0.12f, 0.07f, 0.12f),
                flowerMaterial, false);
        }

        for (int i = 0; i < 7; i++)
        {
            float side = random.Next(0, 2) == 0 ? -1f : 1f;
            float x = side * (9f + (float)random.NextDouble() * 35f);
            float z = startZ + (float)random.NextDouble() * length;
            float size = 0.7f + (float)random.NextDouble() * 1.4f;
            CreatePrimitive(parent, PrimitiveType.Sphere, "Forest_Rock",
                new Vector3(x, size * 0.3f, z), new Vector3(size * 1.4f, size * 0.7f, size),
                rockMaterial, true);

            if (i < 3)
            {
                GameObject branch = CreatePrimitive(parent, PrimitiveType.Cylinder, "Forest_FallenBranch",
                    new Vector3(x + side * 2f, 0.3f, z + 2f), new Vector3(0.2f, 1.8f, 0.2f),
                    trunkMaterial, true);
                branch.transform.rotation = Quaternion.Euler(0f, random.Next(0, 360), 90f);
            }
        }
    }

    private void SpawnTree(Transform parent, System.Random random, int side, float z)
    {
        float x = side * (8f + (float)random.NextDouble() * 38f);
        float height = 8f + (float)random.NextDouble() * 9f;

        if (treePrefab != null)
        {
            GameObject tree = Instantiate(treePrefab, new Vector3(x, 0f, z),
                Quaternion.Euler(0f, random.Next(0, 360), 0f), parent);
            tree.name = "Forest_Tree";
            return;
        }

        float trunkRadius = 0.22f + (float)random.NextDouble() * 0.17f;
        GameObject trunk = CreatePrimitive(parent, PrimitiveType.Cylinder, "Forest_TreeTrunk",
            new Vector3(x, height * 0.5f, z), new Vector3(trunkRadius, height * 0.5f, trunkRadius),
            trunkMaterial, true);
        trunk.transform.rotation = Quaternion.Euler(0f, random.Next(0, 360), 0f);
        CreatePrimitive(parent, PrimitiveType.Cylinder, "Forest_TreeRootFlare",
            new Vector3(x, 0.48f, z), new Vector3(trunkRadius * 1.65f, 0.48f, trunkRadius * 1.65f),
            trunkMaterial, false);

        bool conifer = random.NextDouble() < 0.72;
        if (conifer)
        {
            SpawnConiferCanopy(parent, random, x, z, height);
            return;
        }

        SpawnBroadleafCanopy(parent, random, x, z, height);
    }

    private void SpawnConiferCanopy(Transform parent, System.Random random, float x, float z, float height)
    {
        float canopyWidth = 2.2f + height * 0.15f;
        int tiers = 4;
        int palette = random.Next(coniferMaterials.Length);
        for (int tier = 0; tier < tiers; tier++)
        {
            float tierHeight = height * (0.30f - tier * 0.025f);
            float radius = canopyWidth * (1f - tier * 0.17f);
            float centerY = height * (0.43f + tier * 0.12f);
            GameObject foliage = CreateCone(parent, "Forest_ConiferBoughs",
                new Vector3(x, centerY, z), new Vector3(radius * 2f, tierHeight, radius * 2f),
                coniferMaterials[(palette + tier) % coniferMaterials.Length]);
            foliage.transform.rotation = Quaternion.Euler(0f, random.Next(0, 360), 0f);
        }
    }

    private void SpawnBroadleafCanopy(Transform parent, System.Random random, float x, float z, float height)
    {
        float crownWidth = 2.8f + height * 0.12f;
        int palette = random.Next(canopyMaterials.Length);
        for (int branch = 0; branch < 4; branch++)
        {
            float angle = branch * 90f + random.Next(-24, 25);
            float radians = angle * Mathf.Deg2Rad;
            Vector3 start = new Vector3(x, height * 0.53f, z);
            Vector3 end = start + new Vector3(Mathf.Sin(radians), 0.6f, Mathf.Cos(radians)) * crownWidth * 0.58f;
            CreateBranch(parent, "Forest_TreeBranch", start, end, 0.11f, trunkMaterial);
        }

        for (int cluster = 0; cluster < 5; cluster++)
        {
            float angle = cluster * 72f + random.Next(-20, 21);
            float radians = angle * Mathf.Deg2Rad;
            float spread = cluster == 0 ? 0f : crownWidth * 0.34f;
            float scale = 0.72f + (float)random.NextDouble() * 0.55f;
            Vector3 position = new Vector3(
                x + Mathf.Sin(radians) * spread,
                height * (0.75f + (float)random.NextDouble() * 0.10f),
                z + Mathf.Cos(radians) * spread);
            GameObject foliage = CreatePrimitive(parent, PrimitiveType.Sphere, "Forest_BroadleafCrown",
                position, new Vector3(crownWidth * scale, 2.5f + height * 0.1f, crownWidth * scale),
                canopyMaterials[(palette + cluster) % canopyMaterials.Length], false);
            foliage.transform.rotation = Quaternion.Euler(random.Next(-12, 13), angle, random.Next(-12, 13));
        }
    }

    private void SpawnBush(Transform parent, System.Random random, float x, float z, float size)
    {
        int palette = random.Next(bushMaterials.Length);
        int clusters = 3 + random.Next(3);
        for (int i = 0; i < clusters; i++)
        {
            float spread = size * 0.34f;
            Vector3 position = new Vector3(
                x + (float)(random.NextDouble() * 2.0 - 1.0) * spread,
                size * (0.24f + (float)random.NextDouble() * 0.17f),
                z + (float)(random.NextDouble() * 2.0 - 1.0) * spread);
            float width = size * (0.58f + (float)random.NextDouble() * 0.38f);
            GameObject foliage = CreatePrimitive(parent, PrimitiveType.Sphere, "Forest_Understory",
                position, new Vector3(width, width * 0.56f, width), bushMaterials[(palette + i) % bushMaterials.Length],
                false);
            foliage.transform.rotation = Quaternion.Euler(0f, random.Next(0, 360), 0f);
        }
    }

    private static void CreateBranch(Transform parent, string name, Vector3 start, Vector3 end,
        float radius, Material material)
    {
        Vector3 direction = end - start;
        GameObject branch = CreatePrimitive(parent, PrimitiveType.Cylinder, name,
            (start + end) * 0.5f, new Vector3(radius, direction.magnitude * 0.5f, radius),
            material, false);
        branch.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
    }

    private static GameObject CreateCone(Transform parent, string name, Vector3 position, Vector3 scale,
        Material material)
    {
        GameObject cone = new GameObject(name);
        cone.transform.SetParent(parent, true);
        cone.transform.position = position;
        cone.transform.localScale = scale;
        MeshFilter filter = cone.AddComponent<MeshFilter>();
        filter.sharedMesh = GetConiferMesh();
        MeshRenderer renderer = cone.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return cone;
    }

    private static Mesh GetConiferMesh()
    {
        if (coniferMesh != null)
            return coniferMesh;

        Mesh mesh = new Mesh();
        mesh.name = "Procedural_ConiferBoughMesh";
        const int segments = 12;
        Vector3[] vertices = new Vector3[segments + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[segments * 6];
        vertices[0] = new Vector3(0f, 0.5f, 0f);
        uv[0] = new Vector2(0.5f, 1f);

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle) * 0.5f;
            float z = Mathf.Sin(angle) * 0.5f;
            vertices[i + 1] = new Vector3(x, -0.5f, z);
            uv[i + 1] = new Vector2(i / (float)segments, 0f);

            int next = (i + 1) % segments + 1;
            int sideIndex = i * 3;
            triangles[sideIndex] = 0;
            triangles[sideIndex + 1] = next;
            triangles[sideIndex + 2] = i + 1;
            int baseIndex = segments * 3 + sideIndex;
            triangles[baseIndex] = segments + 1;
            triangles[baseIndex + 1] = i + 1;
            triangles[baseIndex + 2] = next;
        }

        vertices[segments + 1] = new Vector3(0f, -0.5f, 0f);
        uv[segments + 1] = new Vector2(0.5f, 0.5f);
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        coniferMesh = mesh;
        return coniferMesh;
    }

    private void CreateBridge(Transform parent, float streamSide, float z)
    {
        float x = streamSide * 30f;
        CreatePrimitive(parent, PrimitiveType.Cube, "Forest_BridgeDeck",
            new Vector3(x, 0.11f, z), new Vector3(3.4f, 0.16f, 6f), trunkMaterial, true);
        CreatePrimitive(parent, PrimitiveType.Cube, "Forest_BridgeRail",
            new Vector3(x - 1.45f, 0.65f, z), new Vector3(0.13f, 0.9f, 6f), trunkMaterial, true);
        CreatePrimitive(parent, PrimitiveType.Cube, "Forest_BridgeRail",
            new Vector3(x + 1.45f, 0.65f, z), new Vector3(0.13f, 0.9f, 6f), trunkMaterial, true);
    }

    private void EnsureMaterials()
    {
        if (trunkMaterial != null)
            return;
        trunkMaterial = CreateMaterial(new Color(0.19f, 0.13f, 0.10f));
        coniferMaterials = new[]
        {
            CreateMaterial(new Color(0.075f, 0.16f, 0.105f)),
            CreateMaterial(new Color(0.095f, 0.19f, 0.12f)),
            CreateMaterial(new Color(0.115f, 0.21f, 0.13f))
        };
        canopyMaterials = new[]
        {
            CreateMaterial(new Color(0.13f, 0.21f, 0.12f)),
            CreateMaterial(new Color(0.17f, 0.24f, 0.13f)),
            CreateMaterial(new Color(0.20f, 0.25f, 0.15f))
        };
        bushMaterials = new[]
        {
            CreateMaterial(new Color(0.12f, 0.18f, 0.10f)),
            CreateMaterial(new Color(0.17f, 0.22f, 0.11f)),
            CreateMaterial(new Color(0.21f, 0.24f, 0.14f))
        };
        groundMaterial = CreateDirtMaterial();
        rockMaterial = CreateMaterial(new Color(0.20f, 0.22f, 0.21f));
        waterMaterial = CreateMaterial(new Color(0.09f, 0.16f, 0.17f));
        flowerMaterial = CreateMaterial(new Color(0.55f, 0.32f, 0.20f));
    }

    private static GameObject CreatePrimitive(Transform parent, PrimitiveType type, string objectName,
        Vector3 position, Vector3 scale, Material material, bool keepCollider)
    {
        GameObject item = GameObject.CreatePrimitive(type);
        item.name = objectName;
        item.transform.SetParent(parent, true);
        item.transform.position = position;
        item.transform.localScale = scale;
        Renderer renderer = item.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.sharedMaterial = material;
        Collider collider = item.GetComponent<Collider>();
        if (collider != null && !keepCollider)
            collider.enabled = false;
        return item;
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

    private static Material CreateDirtMaterial()
    {
        bool usingScriptablePipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
        Shader shader = Shader.Find(usingScriptablePipeline ? "Universal Render Pipeline/Lit" : "Standard");
        if (shader == null)
            shader = Shader.Find(usingScriptablePipeline ? "Standard" : "Universal Render Pipeline/Lit");
        if (shader == null)
            return null;

        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, true);
        texture.name = "Procedural_MuddyTrail";
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float broad = ValueNoise(x / 32f, y / 32f);
                float medium = ValueNoise(x / 9f, y / 9f);
                float grain = ValueNoise(x / 2.3f, y / 2.3f);
                float variation = (broad * 0.42f + medium * 0.36f + grain * 0.22f - 0.5f) * 0.5f;
                Color soil = new Color(0.39f, 0.25f, 0.15f);
                texture.SetPixel(x, y, new Color(
                    Mathf.Clamp01(soil.r + variation),
                    Mathf.Clamp01(soil.g + variation * 0.75f),
                    Mathf.Clamp01(soil.b + variation * 0.52f)));
            }
        }
        texture.Apply(true, true);

        Material material = new Material(shader);
        material.name = "Procedural_MuddyTrail";
        material.mainTexture = texture;
        material.mainTextureScale = new Vector2(3f, 12f);
        return material;
    }

    private static float ValueNoise(float x, float y)
    {
        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);
        float tx = x - x0;
        float ty = y - y0;
        tx = tx * tx * (3f - 2f * tx);
        ty = ty * ty * (3f - 2f * ty);
        float a = HashNoise(x0, y0);
        float b = HashNoise(x0 + 1, y0);
        float c = HashNoise(x0, y0 + 1);
        float d = HashNoise(x0 + 1, y0 + 1);
        return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
    }

    private static float HashNoise(int x, int y)
    {
        unchecked
        {
            int hash = x * 374761393 + y * 668265263;
            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash ^= hash >> 16;
            return (hash & 0x7fffffff) / 2147483647f;
        }
    }
}
