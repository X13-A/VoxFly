using Palmmedia.ReportGenerator.Core;
using SDD.Events;
using System;
using System.Collections;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class WorldGenerator : MonoBehaviour, IEventHandler
{
    [Header("General parameters")]
    [SerializeField] private float grassDepth = 3;

    [SerializeField] private int width = 50;
    [SerializeField] private int depth = 50;
    [SerializeField] private int height = 50;

    [Header("Optimization parameters")]
    [SerializeField] private int brickSize = 8;

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
    [SerializeField] private float threshold = 0.5f; // Seuil pour la g�n�ration des grottes

    [Header("Deep terrain parameters")]
    [SerializeField] private uint deepTerrainSeed;
    [SerializeField] private float deepTerrainAmplitude = 1;
    [SerializeField] private Vector2 deepTerrainScale = new Vector2(3.33f, 3.33f);
    [SerializeField] private Vector2 deepTerrainOffset = new Vector2();

    [Header("Land coverage parameters")]
    [SerializeField] private uint coverageSeed;
    [SerializeField] private Vector2 coverageScale = new Vector2(3.33f, 3.33f);
    [SerializeField] private Vector2 coverageOffset = new Vector2();
    [SerializeField] private float coverage = 0.5f;


    public Texture3D WorldTexture { get; private set; }
    public Texture3D BrickMapTexture { get; private set; }
    public int BrickSize => brickSize;

    [SerializeField] private Texture3D BrickMapPreset;
    [SerializeField] private Texture3D WorldPreset;

    [SerializeField] private Texture3D BrickMapViz;
    [SerializeField] private Texture3D WorldViz;

    private RenderTexture WorldRenderTexture;
    private RenderTexture BrickMapRenderTexture;
    [SerializeField] private Color grassColor;
    [SerializeField] private Color stoneColor;

    [SerializeField] private ComputeShader compute;
    private int computeKernel;

    public bool WorldGenerated { get; private set; }

    #region Events
    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<RequestWorldGeneratorEvent>(GiveWorldGenerator);
        EventManager.Instance.AddListener<SceneLoadedEvent>(RaiseGeneratedEvent);
    }
    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<RequestWorldGeneratorEvent>(GiveWorldGenerator);
        EventManager.Instance.RemoveListener<SceneLoadedEvent>(RaiseGeneratedEvent);
    }

    public void GiveWorldGenerator(RequestWorldGeneratorEvent e)
    {
        EventManager.Instance?.Raise(new GiveWorldGeneratorEvent { generator = this });
    }
    public void RaiseGeneratedEvent()
    {
        EventManager.Instance.Raise(new WorldGeneratedEvent { generator = this });
    }
    public void RaiseGeneratedEvent(SceneLoadedEvent e)
    {
        EventManager.Instance.Raise(new WorldGeneratedEvent { generator = this });
    }
    #endregion

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    void Start()
    {
        computeKernel = compute.FindKernel("CSMain");
        RandomizeSeeds();

        if (WorldPreset != null && BrickMapPreset != null)
        {
            WorldTexture = WorldPreset;
            BrickMapTexture = BrickMapPreset;
            WorldViz = WorldPreset;
            BrickMapViz = BrickMapPreset;
            RaiseGeneratedEvent();
        }
        else
        {
            StartCoroutine(GenerateTerrain_GPU());
        }
    }

    public void RandomizeSeeds()
    {
        cavesSeed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        coverageSeed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        deepTerrainSeed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        terrainSeed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
    }

    // 50x faster than GenerateTerrain_CPU (RTX 4050, 60W - R9 7940HS, 35W)
    public IEnumerator GenerateTerrain_GPU(Action callback = null, bool log = false)
    {
        WorldGenerated = false;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        if (log)
        {
            UnityEngine.Debug.Log("Starting world generation (GPU)...");
        }

        // Create a RenderTexture with 3D support and enable random write
        RenderTextureDescriptor worldDesc = new RenderTextureDescriptor
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
        WorldRenderTexture = new RenderTexture(worldDesc);
        WorldRenderTexture.Create();

        RenderTextureDescriptor brickMapDesc = new RenderTextureDescriptor
        {
            width = width / brickSize,
            height = height / brickSize,
            volumeDepth = depth / brickSize,
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            enableRandomWrite = true,
            graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm,
            msaaSamples = 1
        };
        BrickMapRenderTexture = new RenderTexture(brickMapDesc);
        BrickMapRenderTexture.Create();

        // Bind the texture to the compute shader
        compute.SetTexture(computeKernel, "WorldTexture", WorldRenderTexture);
        compute.SetTexture(computeKernel, "BrickMap", BrickMapRenderTexture);

        // Set uniforms
        //int[] worldSize = new int[] { width, height, depth };
        compute.SetInts("_WorldSize", new int[] { WorldRenderTexture.width, WorldRenderTexture.height, WorldRenderTexture.volumeDepth });
        compute.SetInts("_BrickSize", brickSize);
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

        // Coverage
        compute.SetFloat("_Coverage", coverage);
        compute.SetVector("_CoverageScale", coverageScale);
        compute.SetVector("_CoverageOffset", coverageOffset);
        compute.SetInt("_CoverageSeed", (int) coverageSeed);

        // Dispatch the compute shader
        int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(height / 8.0f);
        int threadGroupsZ = Mathf.CeilToInt(depth / 8.0f);
        compute.Dispatch(computeKernel, threadGroupsX, threadGroupsY, threadGroupsZ);

        stopwatch.Stop();
        if (log)
        {
            float generationTime = (float)stopwatch.Elapsed.TotalSeconds;
            UnityEngine.Debug.Log($"World generated in {generationTime} seconds, converting it to readable Texture3D...");
        }
        stopwatch.Restart();

        // Convert RenderTexture to Texture3D.
        StartCoroutine(RenderingUtils.ConvertRenderTextureToTexture3D(WorldRenderTexture, (Texture3D tex) =>
        {
            WorldTexture = tex;
            WorldViz = tex;
            WorldRenderTexture.Release();
            //AssetDatabase.CreateAsset(tex, "Assets/IslandWorld.asset");
            
            StartCoroutine(RenderingUtils.ConvertRenderTextureToTexture3D(BrickMapRenderTexture, (Texture3D tex) =>
            {
                BrickMapTexture = tex;
                BrickMapViz = tex;
                BrickMapRenderTexture.Release();
                WorldGenerated = true;
                //AssetDatabase.CreateAsset(tex, "Assets/IslandWorld_BrickMap.asset");
            }));
        }));
        
        while (!WorldGenerated)
        {
            yield return null;
        }

        stopwatch.Stop();
        if (log)
        {
            float conversionTime = (float)stopwatch.Elapsed.TotalSeconds;
            UnityEngine.Debug.Log($"Conversion done in {conversionTime} seconds!");
        }
        RaiseGeneratedEvent();
        callback?.Invoke();
    }

    #region CPU generation
    public static float Get2DNoise(float x, float z, Vector2 scale, Vector2 offset)
    {
        Vector2 adjustedScale = new Vector3(scale.x, scale.y) / 983.3546789f; // Pour �viter les valeurs enti�res qui sont toujours les m�mes avec Mathf.PerlinNoise
        return Mathf.PerlinNoise(offset.x + x * adjustedScale.x, offset.y + z * adjustedScale.y);
    }

    public static float Get3DNoise(float x, float y, float z, Vector3 scale, Vector3 offset)
    {
        Vector3 adjustedScale = new Vector3(scale.x, scale.y, scale.z) / 983.3546789f; // Pour �viter les valeurs enti�res qui sont toujours les m�mes avec Mathf.PerlinNoise
        float ab = Mathf.PerlinNoise(offset.x + x * adjustedScale.x, offset.y + y * adjustedScale.y);
        float bc = Mathf.PerlinNoise(offset.y + y * adjustedScale.y, offset.z + z * adjustedScale.z);
        float ac = Mathf.PerlinNoise(offset.x + x * adjustedScale.x, offset.z + z * adjustedScale.z);

        float ba = Mathf.PerlinNoise(offset.y + y * adjustedScale.y, offset.x + x * adjustedScale.x);
        float cb = Mathf.PerlinNoise(offset.z + z * adjustedScale.z, offset.y + y * adjustedScale.y);
        float ca = Mathf.PerlinNoise(offset.z + z * adjustedScale.z, offset.x + x * adjustedScale.x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }

    public void GenerateTerrain_CPU()
    {
        WorldGenerated = false;
        UnityEngine.Debug.Log("Starting world generation (CPU)...");
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        WorldTexture = new Texture3D(width, height, depth, TextureFormat.R8, false); // R8 car on a besoin seulement d'un channel, et peu de valeurs diff�rentes.
        WorldTexture.anisoLevel = 0;
        WorldTexture.filterMode = FilterMode.Point;
        WorldTexture.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                float currentHeight = terrainStartY + Get2DNoise(x, z, terrainScale, terrainOffset) * terrainAmplitude;

                // S'arr�te d�s qu'on atteint la hauteur actuelle pour ne pas g�n�rer des grottes au dessus du terrain
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

        stopwatch.Stop(); // Stop the timer after compute shader dispatch
        float generationTime = (float)stopwatch.Elapsed.TotalSeconds;
        UnityEngine.Debug.Log($"World generated in {generationTime} seconds (CPU).");

        WorldTexture.Apply();
        WorldGenerated = true;
        RaiseGeneratedEvent();
    }

    #endregion
    
    #region sample
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
        Color res = WorldTexture.GetPixel((int)pos.x, (int)pos.y, (int)pos.z);
        if (res.a == 0) return 0;

        int blockID = (int)Mathf.Round(res.r * 255);
        return blockID;
    }

    public struct RayWorldIntersectionInfo
    {
        public float dstToWorld;
        public float dstInsideWorld;

        public RayWorldIntersectionInfo(float dstToWorld, float dstInsideWorld)
        {
            this.dstToWorld = dstToWorld;
            this.dstInsideWorld = dstInsideWorld;
        }
    }

    private float DstToPlane(Vector3 rayOrigin, Vector3 rayDir, float planeY)
    {
        // Check if the plane is parallel to the ray
        if (rayDir.y == 0)
        {
            return -1.0f;
        }

        float t = (planeY - rayOrigin.y) / rayDir.y;

        // Check if the plane is behind the ray's origin
        if (t < 0)
        {
            return -1.0f;
        }

        return t;
    }

    public RayWorldIntersectionInfo RayWorldHit(Vector3 pos, Vector3 dir)
    {
        float dstTop = DstToPlane(pos, dir, WorldTexture.height);
        float dstBottom = DstToPlane(pos, dir, 0);

        float dstToWorld;
        float dstInsideWorld;

        // If inside the world
        if (IsInWorld(pos))
        {
            dstToWorld = 0;
            // Check if direction is parallel to the planes
            if (dir.y == 0) return new RayWorldIntersectionInfo(dstToWorld, float.PositiveInfinity);
            // Return dist inside world
            return new RayWorldIntersectionInfo(dstToWorld, Mathf.Max(dstTop, dstBottom));
        }

        // If above the world
        if (pos.y > WorldTexture.height)
        {
            // Check if looking at world
            if (dstTop < 0) return new RayWorldIntersectionInfo(-1, -1);

            dstInsideWorld = dstBottom - dstTop;
            return new RayWorldIntersectionInfo(dstTop, dstInsideWorld);
        }
        // If under the world
        else
        {
            // Check if looking at world
            if (dstBottom < 0) return new RayWorldIntersectionInfo(-1, -1);

            dstInsideWorld = dstTop - dstBottom;
            return new RayWorldIntersectionInfo(dstBottom, dstInsideWorld);
        }
    }

    // TODO: Handle normals when needed
    public struct RayWorldInfo
    {
        public bool hit;
        public int BlockID;
        public float depth;
        public Vector3 pos;
    }

    public RayWorldInfo RayCastWorld(Vector3 pos, Vector3 dir, float maxRange = 1000f, int maxIterations = 1000)
    {
        Vector3 startPos = pos;
        Vector3 rayDir = dir;

        RayWorldInfo res = new RayWorldInfo();

        RayWorldIntersectionInfo rayWorldInfo = RayWorldHit(startPos, rayDir);
        float dstToWorld = rayWorldInfo.dstToWorld;
        float dstInsideWorld = rayWorldInfo.dstInsideWorld;

        // EXIT EARLY
        if (dstInsideWorld <= 0)
        {
            res.hit = false;
            res.BlockID = 0;
            res.depth = float.PositiveInfinity;
            res.pos = new Vector3();
            return res;
        }
        if (dstToWorld > 0)
        {
            startPos = startPos + rayDir * dstToWorld; // Start at intersection point
        }

        Vector3Int voxelIndex = new Vector3Int((int)Mathf.Round(startPos.x), (int)Mathf.Round(startPos.y), (int)Mathf.Round(startPos.z));

        Vector3Int step = new Vector3Int((int)Mathf.Sign(rayDir.x), (int)Mathf.Sign(rayDir.y), (int)Mathf.Sign(rayDir.z));
        Vector3 tMax = new Vector3();  // Distance to next voxel boundary
        Vector3 tDelta = new Vector3();  // How far we travel to cross a voxel

        // Calculate initial tMax and tDelta for each axis
        for (int i = 0; i < 3; i++)
        {
            if (rayDir[i] == 0)
            {
                tMax[i] = float.PositiveInfinity;
                tDelta[i] = float.PositiveInfinity;
            }
            else
            {
                float voxelBoundary = voxelIndex[i] + (step[i] > 0 ? 1 : 0);
                tMax[i] = (voxelBoundary - startPos[i]) / rayDir[i];
                tDelta[i] = Mathf.Abs(1.0f / rayDir[i]);
            }
        }

        float dstLimit = Mathf.Min(maxRange - dstToWorld, dstInsideWorld);
        float dstTravelled = 0;
        int loopCount = 0;
        int hardLoopLimit = (int) Mathf.Min(dstLimit * 2, maxIterations); // Hack that prevents convergence caused by precision issues or some dark mathematical magic

        while (loopCount < hardLoopLimit && dstTravelled < dstLimit)
        {
            // Check the position for a voxel
            Vector3 rayPos = startPos + rayDir * (dstTravelled + 0.001f);
            Vector3Int sampledVoxelIndex = new Vector3Int((int)Mathf.Round(rayPos.x), (int)Mathf.Round(rayPos.y), (int)Mathf.Round(rayPos.z));
            int blockID = SampleWorld(sampledVoxelIndex);

            // Return the voxel
            if (blockID > 0)
            {
                res.hit = true;
                res.BlockID = blockID;
                res.depth = dstTravelled;
                res.pos = rayPos;
                return res;
            }

            // Move to the next voxel
            if (tMax.x < tMax.y && tMax.x < tMax.z)
            {
                dstTravelled = tMax.x;
                tMax.x += tDelta.x;
                voxelIndex.x += step.x;
            }
            else if (tMax.y < tMax.z)
            {
                dstTravelled = tMax.y;
                tMax.y += tDelta.y;
                voxelIndex.y += step.y;
            }
            else
            {
                dstTravelled = tMax.z;
                tMax.z += tDelta.z;
                voxelIndex.z += step.z;
            }
            loopCount++;
        }
        res.hit = false;
        res.BlockID = 0;
        res.depth = float.PositiveInfinity;
        res.pos = new Vector3();
        return res;
    }

    #endregion
    
    #region edit
    private Vector3Int GetGridPos(Vector3 pos)
    {
        Vector3Int gridPos = new Vector3Int((int)Math.Round(pos.x), (int)Math.Round(pos.y), (int)Math.Round(pos.z));
        gridPos.x = gridPos.x % Size.x;
        if (gridPos.x < 0) gridPos.x += Size.x;
        gridPos.y = Math.Clamp(gridPos.y, 0, Size.y);
        gridPos.z = gridPos.z % Size.z;
        if (gridPos.z < 0) gridPos.z += Size.z;
        return gridPos;
    }
    public bool RemoveBlock(Vector3 position)
    {
        Vector3Int gridPos = GetGridPos(position);
        float pixel = WorldTexture.GetPixel(gridPos.x, gridPos.y, gridPos.z).r;
        if (pixel <= 0)
        {
            return false;
        }
        WorldTexture.SetPixel(gridPos.x, gridPos.y, gridPos.z, Color.clear);
        return true;
    }

    public void ApplyChanges()
    {
        WorldTexture.Apply();
    }
    #endregion
}
