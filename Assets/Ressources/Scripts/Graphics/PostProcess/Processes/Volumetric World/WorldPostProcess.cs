using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEditor;
using UnityEngine;

public class WorldPostProcess : PostProcessBase
{
    [Header("Player")]
    [SerializeField] private Transform playerLight;
    [SerializeField][Range(0, 200f)] private float playerLightRange;
    [SerializeField][Range(0, 5f)] private float playerLightIntensity;
    [SerializeField][Range(0, 1f)] public float playerLightVolumetricIntensity;
    [SerializeField][Range(0, 90f)] private float playerLightAngle;

    [Header("Pipeline")]
    [SerializeField] private Material postProcessMaterial;

    [Header("Volumetric Lighting Parameters")]
    [SerializeField][Range(0, 200)] private int lightShaftSampleCount;
    [SerializeField][Range(0f, 1000f)] private float lightShaftRenderDistance;
    [SerializeField][Range(0f, 1f)] private float lightShaftFadeStart;
    [SerializeField][Range(0f, 5f)] private float lightShaftIntensity;
    [SerializeField][Range(0f, 1f)] private float lightShaftMaximumValue;

    [Header("Textures")]
    [SerializeField] private List<Texture2D> blockTextures;
    [SerializeField] private Texture2D textureAtlas;
    [SerializeField] private Texture2D noiseTexture;

    [Header("Debug")]
    [SerializeField] private bool debugToggle;
    
    public Texture3D WorldTexture => generator.WorldTexture;

    private GBuffer gBuffer;
    private WorldGenerator generator;
    private ShadowMap shadowMap;

    #region Events

    private void OnWorldGenerated(WorldGeneratedEvent e)
    {
        generator = e.generator;
    }

    private void AttachGBuffer(GBufferInitializedEvent e)
    {
        gBuffer = e.gbuffer;
    }
    private void AttachShadowMap(ShadowMapInitializedEvent e)
    {
        shadowMap = e.shadowMap;
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GBufferInitializedEvent>(AttachGBuffer);
        EventManager.Instance.AddListener<ShadowMapInitializedEvent>(AttachShadowMap);
        EventManager.Instance.AddListener<WorldGeneratedEvent>(OnWorldGenerated);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GBufferInitializedEvent>(AttachGBuffer);
        EventManager.Instance.RemoveListener<ShadowMapInitializedEvent>(AttachShadowMap);
        EventManager.Instance.RemoveListener<WorldGeneratedEvent>(OnWorldGenerated);

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

        // Camera
        postProcessMaterial.SetVector("_CameraPos", Camera.main.transform.position);

        // Player light
        postProcessMaterial.SetVector("_PlayerLightDir", playerLight.forward);
        postProcessMaterial.SetVector("_PlayerLightPos", playerLight.position);
        postProcessMaterial.SetFloat("_PlayerLightIntensity", playerLightIntensity);
        postProcessMaterial.SetFloat("_PlayerLightVolumetricIntensity", playerLightVolumetricIntensity);
        postProcessMaterial.SetFloat("_PlayerLightRange", playerLightRange);
        postProcessMaterial.SetFloat("_PlayerLightAngle", playerLightAngle);
    }

    public override void Apply(RenderTexture source, RenderTexture dest)
    {
        if (gBuffer != null && generator != null && shadowMap != null && postProcessMaterial != null && Camera.current != null)
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
