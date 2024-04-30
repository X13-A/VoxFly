using SDD.Events;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("General parameters")]
    [SerializeField] private float grassDepth = 3;
    [SerializeField] private bool refresh;

    [SerializeField] private int width = 50;
    [SerializeField] private int depth = 50;
    [SerializeField] private int height = 50;

    public Vector3Int Size => new Vector3Int(width, height, depth);

    [Header("Terrain parameters")]
    [SerializeField] private uint terrainSeed;
    [SerializeField] private float terrainAmplitude = 1;
    [SerializeField] private float terrainStartY = 15;
    [SerializeField] private Vector2 terrainScale = new Vector2(3.33f, 3.33f);
    [SerializeField] private Vector2 terrainOffset = new Vector2();

    [Header("Caves parameters")]
    [SerializeField] private uint cavesSeed;
    [SerializeField] private Vector3 offset = new Vector3();
    [SerializeField] private Vector3 scale = new Vector3(3.33f, 3.33f, 3.33f);
    [SerializeField] private float threshold = 0.5f; // Seuil pour la génération des grottes

    [Header("Deep terrain parameters")]
    [SerializeField] private uint deepTerrainSeed;
    [SerializeField] private float deepTerrainAmplitude = 1;
    [SerializeField] private Vector2 deepTerrainScale = new Vector2(3.33f, 3.33f);
    [SerializeField] private Vector2 deepTerrainOffset = new Vector2();

    public Texture3D WorldTexture { get; private set; }

    [SerializeField] private RenderTexture WorldRenderTexture;
    [SerializeField] private Color grassColor;
    [SerializeField] private Color stoneColor;

    [SerializeField] private ComputeShader compute;
    private int computeKernel;


    public bool WorldGenerated { get; private set; }
    void Start()
    {
        computeKernel = compute.FindKernel("CSMain");
        GenerateTerrain_GPU();
    }

    public void Refresh()
    {
        refresh = true;
    }

    public void GenerateTerrain_CPU()
    {
        WorldGenerated = false;
        UnityEngine.Debug.Log("Starting world generation (CPU)...");
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        WorldTexture = new Texture3D(width, height, depth, TextureFormat.R8, false); // R8 car on a besoin seulement d'un channel, et peu de valeurs différentes.
        WorldTexture.anisoLevel = 0;
        WorldTexture.filterMode = FilterMode.Point;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float currentHeight = terrainStartY + Get2DNoise(x, z, terrainScale, terrainOffset) * terrainAmplitude;

                // S'arrète dès qu'on atteint la hauteur actuelle pour ne pas générer des grottes au dessus du terrain
                for (int y = 0; y < height; y++)
                {
                    if (y >= currentHeight)
                    {
                        WorldTexture.SetPixel(x, y, z, Color.clear);
                        continue;
                    }

                    float noise = Get3DNoise(x, y, z, scale, offset);
                    Color colorUsed = Mathf.Abs(currentHeight - y) < grassDepth ? grassColor : stoneColor;
                    if (noise > threshold)
                    {
                        WorldTexture.SetPixel(x, y, z, colorUsed);
                        continue;
                    }
                    WorldTexture.SetPixel(x, y, z, Color.clear);
                }
            }
        }

        // Fill bottom
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                WorldTexture.SetPixel(x, 0, z, stoneColor);
                WorldTexture.SetPixel(x, 1, z, stoneColor);
                WorldTexture.SetPixel(x, 2, z, stoneColor);
            }
        }

        // Debug: Create tower at origin
        //for (int y = 0; y < height - 1; y++)
        //{
        //    for (int x = 0; x < 2; x++)
        //    {
        //        for (int z = 0; z < 2; z++)
        //        {
        //            WorldTexture.SetPixel(x, y, z, stoneColor);
        //        }
        //    }
        //}

        stopwatch.Stop(); // Stop the timer after compute shader dispatch
        float generationTime = (float)stopwatch.Elapsed.TotalSeconds;
        UnityEngine.Debug.Log($"World generated in {generationTime} seconds (CPU).");

        WorldTexture.Apply();
        WorldGenerated = true;
    }

    // 50x faster than GenerateTerrain_CPU (RTX 4050, 60W - R9 7940HS, 35W)
    public void GenerateTerrain_GPU()
    {
        WorldGenerated = false;
        UnityEngine.Debug.Log("Starting world generation (GPU)...");
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // Create a RenderTexture with 3D support and enable random write
        RenderTextureDescriptor desc = new RenderTextureDescriptor
        {
            width = width,
            height = height,
            volumeDepth = depth,
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            enableRandomWrite = true,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm,
            msaaSamples = 1
        };

        // Create the new texture based on the descriptor
        if (WorldRenderTexture != null) WorldRenderTexture.Release();
        WorldRenderTexture = new RenderTexture(desc);
        WorldRenderTexture.Create();

        // Bind the texture to the compute shader
        compute.SetTexture(computeKernel, "WorldTexture", WorldRenderTexture);

        // Set uniforms
        compute.SetInts("_WorldSize", width, height, depth);
        compute.SetFloat("_TerrainAmplitude", terrainAmplitude);
        compute.SetFloat("_ElevationStartY", terrainStartY);
        compute.SetVector("_TerrainScale", new Vector4(terrainScale.x, terrainScale.y, 0, 0));
        compute.SetVector("_TerrainOffset", new Vector4(terrainOffset.x, terrainOffset.y, 0, 0));
        compute.SetInt("_TerrainSeed", (int)terrainSeed);

        compute.SetFloat("_DeepTerrainAmplitude", deepTerrainAmplitude);
        compute.SetVector("_DeepTerrainScale", new Vector4(deepTerrainScale.x, deepTerrainScale.y, 0, 0));
        compute.SetVector("_DeepTerrainOffset", new Vector4(deepTerrainOffset.x, deepTerrainOffset.y, 0, 0));
        compute.SetInt("_DeepTerrainSeed", (int)deepTerrainSeed);

        compute.SetVector("_GrassColor", grassColor);
        compute.SetVector("_StoneColor", stoneColor);
        compute.SetFloat("_GrassDepth", grassDepth);
        compute.SetFloat("_CavesThreshold", threshold);
        compute.SetVector("_CavesScale", scale);
        compute.SetVector("_CavesOffset", offset);
        compute.SetInt("_CavesSeed", (int) cavesSeed);

        // Dispatch the compute shader
        int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(height / 8.0f);
        int threadGroupsZ = Mathf.CeilToInt(depth / 8.0f);
        compute.Dispatch(computeKernel, threadGroupsX, threadGroupsY, threadGroupsZ);

        stopwatch.Stop(); // Stop the timer after compute shader dispatch
        float generationTime = (float)stopwatch.Elapsed.TotalSeconds;
        UnityEngine.Debug.Log($"World generated in {generationTime} seconds, converting it to readable Texture3D...");

        stopwatch.Restart();
        // Convert RenderTexture to Texture3D.
        StartCoroutine(RenderingUtils.ConvertRenderTextureToTexture3D(WorldRenderTexture, (Texture3D tex) =>
        {
            WorldTexture = tex;
            stopwatch.Stop(); // Stop the timer after conversion is complete
            float conversionTime = (float)stopwatch.Elapsed.TotalSeconds;
            UnityEngine.Debug.Log($"Conversion done in {conversionTime} seconds!");
            WorldRenderTexture.Release();
            WorldGenerated = true;
            EventManager.Instance?.Raise(new WorldGeneratedEvent());
        }));
    }

    public static float Get2DNoise(float x, float z, Vector2 scale, Vector2 offset)
    {
        Vector2 adjustedScale = new Vector3(scale.x, scale.y) / 983.3546789f; // Pour éviter les valeurs entières qui sont toujours les mêmes avec Mathf.PerlinNoise
        return Mathf.PerlinNoise(offset.x + x * adjustedScale.x, offset.y + z * adjustedScale.y);
    }

    public static float Get3DNoise(float x, float y, float z, Vector3 scale, Vector3 offset)
    {
        Vector3 adjustedScale = new Vector3(scale.x, scale.y, scale.z) / 983.3546789f; // Pour éviter les valeurs entières qui sont toujours les mêmes avec Mathf.PerlinNoise
        float ab = Mathf.PerlinNoise(offset.x + x * adjustedScale.x, offset.y + y * adjustedScale.y);
        float bc = Mathf.PerlinNoise(offset.y + y * adjustedScale.y, offset.z + z * adjustedScale.z);
        float ac = Mathf.PerlinNoise(offset.x + x * adjustedScale.x, offset.z + z * adjustedScale.z);

        float ba = Mathf.PerlinNoise(offset.y + y * adjustedScale.y, offset.x + x * adjustedScale.x);
        float cb = Mathf.PerlinNoise(offset.z + z * adjustedScale.z, offset.y + y * adjustedScale.y);
        float ca = Mathf.PerlinNoise(offset.z + z * adjustedScale.z, offset.x + x * adjustedScale.x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }

    public bool IsInWorld(Vector3 pos)
    {
        return pos.y < WorldTexture.height && pos.y >= 0;
    }

    public int SampleWorld(Vector3 pos)
    {
        if (!IsInWorld(pos))
        {
            return 0;
        }
        Color res = WorldTexture.GetPixel((int) pos.x, (int) pos.y, (int) pos.z);
        if (res.a == 0) return 0;

        int blockID = (int) Mathf.Round(res.r * 255); 
        return blockID;
    }

    private void Update()
    {
        if (refresh || Input.GetKeyDown(KeyCode.R))
        {
            GenerateTerrain_GPU();
            refresh = false;
        }
    }
}
