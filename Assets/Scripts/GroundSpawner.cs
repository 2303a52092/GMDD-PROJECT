using System.Collections.Generic;
using UnityEngine;

public sealed class GroundSpawner : MonoBehaviour
{
    [SerializeField, Min(20f)] private float tileLength = 60f;
    [SerializeField, Min(50f)] private float forestWidth = 110f;
    [SerializeField, Min(1)] private int tilesAhead = 3;

    private readonly Dictionary<int, GameObject> tiles = new Dictionary<int, GameObject>();
    private Transform player;
    private ForestGenerator forestGenerator;
    private ObstacleSpawner obstacleSpawner;
    private Material groundMaterial;
    private bool initialized;

    public void Configure(Transform playerTransform, ForestGenerator forest, ObstacleSpawner obstacles)
    {
        player = playerTransform;
        forestGenerator = forest;
        obstacleSpawner = obstacles;
    }

    public void Initialize()
    {
        if (initialized)
            return;
        initialized = true;
        UpdateTiles(true);
    }

    private void Update()
    {
        if (initialized && player != null)
            UpdateTiles(false);
    }

    private void UpdateTiles(bool initial)
    {
        int current = Mathf.FloorToInt(player != null ? player.position.z / tileLength : 0f);
        int first = initial ? -1 : current - 1;
        int last = initial ? tilesAhead - 1 : current + tilesAhead;

        List<int> remove = new List<int>();
        foreach (KeyValuePair<int, GameObject> pair in tiles)
        {
            if (pair.Key < first)
            {
                if (pair.Value != null)
                    Destroy(pair.Value);
                remove.Add(pair.Key);
            }
        }
        for (int i = 0; i < remove.Count; i++)
            tiles.Remove(remove[i]);

        for (int index = first; index <= last; index++)
        {
            if (!tiles.ContainsKey(index))
                SpawnTile(index);
        }
    }

    private void SpawnTile(int index)
    {
        EnsureGroundMaterial();
        GameObject tile = new GameObject("ForestTile_" + index);
        tile.transform.position = new Vector3(0f, 0f, index * tileLength);
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "ForestGround";
        ground.transform.SetParent(tile.transform, false);
        ground.transform.localPosition = new Vector3(0f, -0.55f, 0f);
        ground.transform.localScale = new Vector3(forestWidth, 1f, tileLength);
        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null && groundMaterial != null)
            renderer.sharedMaterial = groundMaterial;

        float startZ = index * tileLength - tileLength * 0.5f;
        if (forestGenerator != null)
            forestGenerator.Populate(tile.transform, index, startZ, tileLength);
        if (obstacleSpawner != null)
            obstacleSpawner.Populate(tile.transform, index, startZ, tileLength);
        tiles.Add(index, tile);
    }

    public void ResetWorld()
    {
        foreach (GameObject tile in tiles.Values)
        {
            if (tile != null)
                Destroy(tile);
        }
        tiles.Clear();
        initialized = true;
        UpdateTiles(true);
    }

    private void EnsureGroundMaterial()
    {
        if (groundMaterial != null)
            return;
        bool usingScriptablePipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
        Shader shader = Shader.Find(usingScriptablePipeline ? "Universal Render Pipeline/Lit" : "Standard");
        if (shader == null)
            shader = Shader.Find(usingScriptablePipeline ? "Standard" : "Universal Render Pipeline/Lit");
        if (shader != null)
        {
            groundMaterial = new Material(shader);
            groundMaterial.color = Color.white;
            groundMaterial.mainTexture = CreateForestFloorTexture();
            groundMaterial.mainTextureScale = new Vector2(18f, 12f);
        }
    }

    private static Texture2D CreateForestFloorTexture()
    {
        const int size = 128;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGB24, true);
        texture.name = "Procedural_ForestFloor";
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        System.Random random = new System.Random(6803);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noise = (float)random.NextDouble();
                float shade = (noise - 0.5f) * 0.12f;
                Color soil = new Color(0.19f + shade, 0.22f + shade, 0.15f + shade * 0.65f);
                if (noise > 0.96f)
                    soil = Color.Lerp(soil, new Color(0.32f, 0.24f, 0.15f), 0.45f);
                pixels[y * size + x] = soil;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(true, true);
        return texture;
    }
}
