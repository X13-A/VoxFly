using UnityEngine;
using System.Collections.Generic;

public class CloudsNoiseGenerator : MonoBehaviour
{
    [SerializeField] private ComputeShader worleyComputeShader;
    [SerializeField] private ComputeShader perlinComputeShader;

    private int worleyKernelHandle;
    private int perlinKernelHandle;

    private void Start()
    {
        worleyKernelHandle = worleyComputeShader.FindKernel("CSMain");
        perlinKernelHandle = perlinComputeShader.FindKernel("CSMain");
    }

    #region Worley noise
    public List<Vector3> CreateWorleyPointsGrid(int gridSize)
    {
        List<Vector3> points = new List<Vector3>();

        float cellSize = 1.0f / gridSize;
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    Vector3 randomOffset = new Vector3(
                        Random.value * cellSize,
                        Random.value * cellSize,
                        Random.value * cellSize
                    );
                    Vector3 cellCorner = new Vector3(x, y, z) * cellSize;
                    Vector3 point = cellCorner + randomOffset;
                    points.Add(point);
                }
            }
        }
        return points;
    }

    public List<Vector3> CreateWorleyPoints(int n)
    {
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < n; i++)
        {
            points.Add(new Vector3(Random.value, Random.value, Random.value));
        }
        return points;
    }

    public List<Vector3> RepeatWorleyPoints(List<Vector3> points)
    {
        List<Vector3> repeatedPoints = new List<Vector3>();
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    Vector3 offset = new Vector3(x, y, z);
                    foreach (Vector3 point in points)
                    {
                        repeatedPoints.Add(point + offset - Vector3.one);
                    }
                }
            }
        }
        return repeatedPoints;
    }

    public RenderTexture ComputeWorleyTexture(List<Vector3> worleyPoints, int textureSize)
    {
        RenderTexture worleyTexture = new RenderTexture(textureSize, textureSize, 0);
        worleyTexture.enableRandomWrite = true;
        worleyTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        worleyTexture.volumeDepth = textureSize;
        worleyTexture.format = RenderTextureFormat.ARGB32;
        worleyTexture.useMipMap = false;
        worleyTexture.anisoLevel = 0;
        worleyTexture.wrapMode = TextureWrapMode.Repeat;
        worleyTexture.Create();

        int bufferStride = sizeof(float) * 3; // 3 floats for Vector3
        ComputeBuffer pointsBuffer = new ComputeBuffer(worleyPoints.Count, bufferStride);
        pointsBuffer.SetData(worleyPoints);

        worleyComputeShader.SetBuffer(worleyKernelHandle, "pointsBuffer", pointsBuffer);
        worleyComputeShader.SetTexture(worleyKernelHandle, "Result", worleyTexture);
        worleyComputeShader.SetInt("pointsBufferLength", worleyPoints.Count);
        worleyComputeShader.SetInts("textureDimensions", textureSize, textureSize, textureSize);
        worleyComputeShader.Dispatch(worleyKernelHandle, textureSize / 8, textureSize / 8, textureSize / 8);

        pointsBuffer.Release();

        return worleyTexture;
    }

    public RenderTexture ComputeWorleyTexture(int n_points, int textureSize)
    {
        List<Vector3> points = CreateWorleyPoints(n_points);
        points = RepeatWorleyPoints(points);
        return ComputeWorleyTexture(points, textureSize);
    }
    #endregion

    #region Perlin noise
    public RenderTexture ComputePerlinTexture(Vector3 scale, Vector3 offset, int textureSize)
    {
        RenderTexture perlinTexture = new RenderTexture(textureSize, textureSize, 0);
        perlinTexture.enableRandomWrite = true;
        perlinTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        perlinTexture.volumeDepth = textureSize;
        perlinTexture.format = RenderTextureFormat.RFloat;
        perlinTexture.useMipMap = false;
        perlinTexture.anisoLevel = 0;
        perlinTexture.wrapMode = TextureWrapMode.Repeat;
        perlinTexture.filterMode = FilterMode.Bilinear;
        perlinTexture.Create();

        perlinComputeShader.SetTexture(perlinKernelHandle, "Result", perlinTexture);
        perlinComputeShader.SetInts("textureDimensions", textureSize, textureSize, textureSize);
        perlinComputeShader.SetFloats("perlinScale", scale.x, scale.y, scale.z);
        perlinComputeShader.SetFloats("perlinOffset", offset.x, offset.y, offset.z);
        perlinComputeShader.Dispatch(perlinKernelHandle, textureSize / 8, textureSize / 8, textureSize / 8);

        return perlinTexture;
    }

    #endregion
}
