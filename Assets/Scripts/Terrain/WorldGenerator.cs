using Palmmedia.ReportGenerator.Core;
using System;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class WorldGenerator : MonoBehaviour
{
    [Header("General parameters")]
    [SerializeField] private float grassDepth = 3;
    [SerializeField] private bool refresh;

    [SerializeField] private int width = 50;
    [SerializeField] private int depth = 50;
    [SerializeField] private int height = 50;

    public Vector3Int Size => new Vector3Int(width, depth, height);

    [Header("Elevation parameters")]
    [SerializeField] private float amplitude = 1;
    [SerializeField] private float elevationStartY = 15;
    [SerializeField] private Vector2 elevationScale = new Vector2(3.33f, 3.33f);
    [SerializeField] private Vector2 elevationOffset = new Vector2();

    [Header("Caves parameters")]
    [SerializeField] private Vector3 offset = new Vector3();
    [SerializeField] private Vector3 scale = new Vector3(3.33f, 3.33f, 3.33f);
    [SerializeField] private float threshold = 0.5f; // Seuil pour la génération de cubes

    public Texture3D WorldTexture { get; private set; }
    [SerializeField] private Color grassColor;
    [SerializeField] private Color stoneColor;
    public void Refresh()
    {
        refresh = true;
    }

    public void GenerateTerrain()
    {
        Debug.Log("Generating terrain");
        WorldTexture = new Texture3D(width, height, depth, TextureFormat.RGBA32, false);
        WorldTexture.anisoLevel = 0;
        WorldTexture.filterMode = FilterMode.Point;

        for (int x = 0; x < width; x++)
        {
            for(int z = 0; z < depth; z++)
            {
                float currentHeight = elevationStartY + Get2DNoise(x, z, elevationScale, elevationOffset) * amplitude;

                // S'arrête dès qu'on atteint la hauteur actuelle pour ne pas générer des grottes au dessus du terrain
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
        WorldTexture.Apply();
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
            return -1;
        }
        Color res = WorldTexture.GetPixel((int) pos.x, (int) pos.y, (int) pos.z);
        if (res.a == 0) return -1;

        int blockID = (int) Mathf.Round(res.r * 255); 
        return blockID;
    }

    private void Update()
    {
        if (refresh)
        {
            GenerateTerrain();
            refresh = false;
        }
    }
}
