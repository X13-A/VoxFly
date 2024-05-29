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
    private WorldConfig config => WorldConfigManager.Instance.CurrentConfig;
    public Vector3Int Size => new Vector3Int(config.width, config.height, config.depth);
    public Texture3D WorldTexture { get; private set; }
    public Texture3D BrickMapTexture { get; private set; }
    public int BrickSize => config.brickSize;

    private RenderTexture WorldRenderTexture;
    private RenderTexture BrickMapRenderTexture;
    private Color grassColor = new Color(1f/255f, 0, 0, 1);
    private Color stoneColor = new Color(2f/255f, 0, 0, 1);

    [SerializeField] private ComputeShader compute;
    private int computeKernel;

    public bool WorldGenerated { get; private set; }

    #region Events
    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<RequestWorldGeneratorEvent>(GiveWorldGenerator);
        EventManager.Instance.AddListener<SceneLoadedEvent>(RaiseGeneratedEvent);
        EventManager.Instance.AddListener<WorldConfigChangedEvent>(HandleWorldConfigChange);
    }
    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<RequestWorldGeneratorEvent>(GiveWorldGenerator);
        EventManager.Instance.RemoveListener<SceneLoadedEvent>(RaiseGeneratedEvent);
        EventManager.Instance.RemoveListener<WorldConfigChangedEvent>(HandleWorldConfigChange);
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

    public void HandleWorldConfigChange(WorldConfigChangedEvent e)
    {
        StartCoroutine(GenerateTerrain_GPU());
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

        //if (WorldPreset != null && BrickMapPreset != null)
        //{
        //    WorldTexture = WorldPreset;
        //    BrickMapTexture = BrickMapPreset;
        //    RaiseGeneratedEvent();
        //}
        //else
        //{
        //}
        StartCoroutine(GenerateTerrain_GPU());
    }

    public void RandomizeSeeds()
    {
        config.cavesSeed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        config.coverageSeed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        config.deepTerrainSeed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
        config.terrainSeed = (uint)UnityEngine.Random.Range(0, int.MaxValue);
    }

    // 50x faster than GenerateTerrain_CPU (RTX 4050, 60W - R9 7940HS, 35W)
    public IEnumerator GenerateTerrain_GPU(Action callback = null, bool log = false)
    {
        RandomizeSeeds();
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
            width = config.width,
            height = config.height,
            volumeDepth = config.depth,
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
            width = config.width / config.brickSize,
            height = config.height / config.brickSize,
            volumeDepth = config.depth / config.brickSize,
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
        compute.SetInts("_BrickSize", config.brickSize);
        compute.SetFloat("_TerrainAmplitude", config.terrainAmplitude);
        compute.SetFloat("_ElevationStartY", config.terrainStartY);
        compute.SetVector("_TerrainScale", new Vector4(config.terrainScale.x, config.terrainScale.y, 0, 0));
        compute.SetVector("_TerrainOffset", new Vector4(config.terrainOffset.x, config.terrainOffset.y, 0, 0));
        compute.SetInt("_TerrainSeed", (int)config.terrainSeed);

        compute.SetFloat("_DeepTerrainAmplitude", config.deepTerrainAmplitude);
        compute.SetVector("_DeepTerrainScale", new Vector4(config.deepTerrainScale.x, config.deepTerrainScale.y, 0, 0));
        compute.SetVector("_DeepTerrainOffset", new Vector4(config.deepTerrainOffset.x, config.deepTerrainOffset.y, 0, 0));
        compute.SetInt("_DeepTerrainSeed", (int)config.deepTerrainSeed);

        compute.SetVector("_GrassColor", grassColor);
        compute.SetVector("_StoneColor", stoneColor);
        compute.SetFloat("_GrassDepth", config.grassDepth);
        compute.SetFloat("_CavesThreshold", config.threshold);
        compute.SetFloat("_CavesHeightDiminution", config.cavesHeightDiminution);
        compute.SetVector("_CavesScale", config.scale);
        compute.SetVector("_CavesOffset", config.offset);
        compute.SetInt("_CavesSeed", (int) config.cavesSeed);

        // Coverage
        compute.SetFloat("_Coverage", config.coverage);
        compute.SetVector("_CoverageScale", config.coverageScale);
        compute.SetVector("_CoverageOffset", config.coverageOffset);
        compute.SetInt("_CoverageSeed", (int) config.coverageSeed);
        compute.SetFloat("_CoverageFactor", config.coverageFactor);

        // Dispatch the compute shader
        int threadGroupsX = Mathf.CeilToInt(config.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(config.height / 8.0f);
        int threadGroupsZ = Mathf.CeilToInt(config.depth / 8.0f);
        compute.Dispatch(computeKernel, threadGroupsX, threadGroupsY, threadGroupsZ);

        stopwatch.Stop();
        if (log)
        {
            float generationTime = (float)stopwatch.Elapsed.TotalSeconds;
            UnityEngine.Debug.Log($"World generated in {generationTime} seconds, converting it to readable Texture3D...");
        }
        stopwatch.Restart();

        // Convert RenderTexture to Texture3D.
        StartCoroutine(RenderingUtils.ConvertRenderTextureToTexture3D(WorldRenderTexture, 1, TextureFormat.R8, TextureWrapMode.Repeat, FilterMode.Point, (Texture3D tex) =>
        {
            WorldTexture = tex;
            WorldRenderTexture.Release();
            //AssetDatabase.CreateAsset(tex, "Assets/IslandWorld.asset");
            
            StartCoroutine(RenderingUtils.ConvertRenderTextureToTexture3D(BrickMapRenderTexture, 1, TextureFormat.R8, TextureWrapMode.Repeat, FilterMode.Point, (Texture3D tex) =>
            {
                BrickMapTexture = tex;
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

    public void DEPRECATED_GenerateTerrain_CPU()
    {
        WorldGenerated = false;
        UnityEngine.Debug.Log("Starting world generation (CPU)...");
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        WorldTexture = new Texture3D(config.width, config.height, config.depth, TextureFormat.R8, false); // R8 car on a besoin seulement d'un channel, et peu de valeurs diff�rentes.
        WorldTexture.anisoLevel = 0;
        WorldTexture.filterMode = FilterMode.Point;
        WorldTexture.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < config.width; x++)
        {
            for (int z = 0; z < config.depth; z++)
            {
                float currentHeight = config.terrainStartY + RenderingUtils.Get2DNoise(x, z, config.terrainScale, config.terrainOffset) * config.terrainAmplitude;

                // S'arr�te d�s qu'on atteint la hauteur actuelle pour ne pas g�n�rer des grottes au dessus du terrain
                for (int y = 0; y < config.height; y++)
                {
                    if (y >= currentHeight)
                    {
                        WorldTexture.SetPixel(x, y, z, Color.clear);
                        continue;
                    }

                    float noise = RenderingUtils.Get3DNoise(x, y, z, config.scale, config.offset);
                    Color colorUsed = Mathf.Abs(currentHeight - y) < config.grassDepth ? grassColor : stoneColor;
                    if (noise > config.threshold)
                    {
                        WorldTexture.SetPixel(x, y, z, colorUsed);
                        continue;
                    }
                    WorldTexture.SetPixel(x, y, z, Color.clear);
                }
            }
        }

        // Fill bottom
        for (int x = 0; x < config.width; x++)
        {
            for (int z = 0; z < config.depth; z++)
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
