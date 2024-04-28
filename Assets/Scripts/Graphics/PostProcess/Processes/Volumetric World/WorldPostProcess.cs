using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEditor;
using UnityEngine;

public class WorldPostProcess : PostProcessBase
{
    [Header("Pipeline")]
    [SerializeField] private GBuffer gBuffer;
    [SerializeField] private Material postProcessMaterial;
    [SerializeField] private WorldGenerator worldGenerator;
    [SerializeField] private ShadowMap shadowMap;

    [Header("Volumetric Lighting Parameters")]
    [SerializeField] [Range(0, 200)] private int lightShaftSampleCount;
    [SerializeField] [Range(0f, 1000f)] private float lightShaftRenderDistance;
    [SerializeField][Range(0f, 1f)] private float lightShaftFadeStart;
    [SerializeField][Range(0f, 5f)] private float lightShaftIntensity;
    [SerializeField][Range(0f, 1f)] private float lightShaftMaximumValue;

    [Header("Textures")]
    [SerializeField] private List<Texture2D> blockTextures;
    [SerializeField] private Texture2D textureAtlas;
    [SerializeField] private Texture2D noiseTexture;

    [Header("Debug")]
    [SerializeField] private bool refresh;
    [SerializeField] private bool debugToggle;

    public Texture3D WorldTexture => worldGenerator.WorldTexture;

    private void Start()
    {
        CreateTextureAtlas();
    }

    void CreateTextureAtlas()
    {
        if (blockTextures.Count == 0) return;

        int textureWidth = blockTextures[0].width;
        int textureHeight = blockTextures[0].height;
        int atlasWidth = textureWidth * blockTextures.Count;
        int atlasHeight = textureHeight;

        textureAtlas = new Texture2D(atlasWidth, atlasHeight, TextureFormat.RGBA32, false);
        textureAtlas.filterMode = FilterMode.Point;

        for (int i = 0; i < blockTextures.Count; i++)
        {
            Texture2D blockTexture = blockTextures[i];
            if (blockTexture.width != textureWidth || blockTexture.height != textureHeight)
            {
                Debug.LogError("Block texture dimensions do not match expected dimensions.");
                continue;
            }

            Color[] pixels = blockTexture.GetPixels();
            textureAtlas.SetPixels(i * textureWidth, 0, textureWidth, textureHeight, pixels);
        }
        textureAtlas.Apply();
    }

    private void SetUniforms()
    {
        // Pass G-Buffer textures
        postProcessMaterial.SetTexture("_DepthTexture", gBuffer.DepthBuffer);
        postProcessMaterial.SetTexture("_PositionTexture", gBuffer.PositionBuffer);
        postProcessMaterial.SetTexture("_BlockTexture", gBuffer.BlockBuffer);
        postProcessMaterial.SetTexture("_NormalTexture", gBuffer.NormalBuffer);

        postProcessMaterial.SetTexture("_WorldTexture", WorldTexture);
        postProcessMaterial.SetVector("_WorldTextureSize", new Vector3(WorldTexture.width, WorldTexture.height, WorldTexture.depth));
        postProcessMaterial.SetFloat("_VoxelRenderDistance", gBuffer.VoxelViewDistance);
        postProcessMaterial.SetTexture("_BlockTextureAtlas", textureAtlas);
        postProcessMaterial.SetInt("_BlocksCount", blockTextures.Count);
        postProcessMaterial.SetInt("_DebugToggle", debugToggle ? 1 : 0);
        postProcessMaterial.SetTexture("_NoiseTexture", noiseTexture);

        // Parameters
        postProcessMaterial.SetFloat("_LightShaftRenderDistance", lightShaftRenderDistance);
        postProcessMaterial.SetFloat("_LightShaftFadeStart", lightShaftFadeStart);
        postProcessMaterial.SetFloat("_LightShaftIntensity", lightShaftIntensity);
        postProcessMaterial.SetFloat("_LightShaftMaximumValue", lightShaftMaximumValue);
        postProcessMaterial.SetInt("_LightShaftSampleCount", lightShaftSampleCount);

        // Shadow map
        postProcessMaterial.SetVector("_LightDir", shadowMap.LightDir);
        postProcessMaterial.SetVector("_LightUp", shadowMap.LightUp);
        postProcessMaterial.SetVector("_LightRight", shadowMap.LightRight);
        postProcessMaterial.SetTexture("_ShadowMap", shadowMap.ShadowMapRenderTexture);
        postProcessMaterial.SetVector("_ShadowMapOrigin", shadowMap.Origin);
        postProcessMaterial.SetVector("_ShadowMapCoverage", new Vector2(shadowMap.MapWidth, shadowMap.MapHeight));
        postProcessMaterial.SetVector("_ShadowMapResolution", new Vector2(shadowMap.TextureWidth, shadowMap.TextureHeight));
    }

    public override void Apply(RenderTexture source, RenderTexture dest)
    {
        if (refresh)
        {
            worldGenerator.GenerateTerrain();
            refresh = false;
        }

        if (postProcessMaterial != null && WorldTexture != null && Camera.current != null)
        {
            SetUniforms();
            Graphics.Blit(source, dest, postProcessMaterial);
        }
        else
        {
            Graphics.Blit(source, dest);
        }
    }
}
