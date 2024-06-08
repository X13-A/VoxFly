using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaterPostProcess : PostProcessBase
{
    [Header("Pipeline")]
    [SerializeField] private Material postProcessMaterial;

    [Header("Parameters")]
    [SerializeField] private float waterLevel;
    [SerializeField] private float waterDensity;
    [SerializeField] private Color waterColor;

    public float WaterLevel => waterLevel;
    public float WaterDensity => waterDensity;
    public Color WaterColor => waterColor;


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
        EventManager.Instance.AddListener<WorldGeneratedEvent>(OnWorldGenerated);
        EventManager.Instance.AddListener<ShadowMapInitializedEvent>(AttachShadowMap);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GBufferInitializedEvent>(AttachGBuffer);
        EventManager.Instance.RemoveListener<WorldGeneratedEvent>(OnWorldGenerated);
        EventManager.Instance.RemoveListener<ShadowMapInitializedEvent>(AttachShadowMap);
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
    private void SetUniforms()
    {
        // Pass G-Buffer textures
        postProcessMaterial.SetTexture("_DepthTexture", gBuffer.DepthBuffer);
        postProcessMaterial.SetTexture("_PositionTexture", gBuffer.PositionBuffer);
        //postProcessMaterial.SetTexture("_BlockTexture", gBuffer.BlockBuffer);
        //postProcessMaterial.SetTexture("_NormalTexture", gBuffer.NormalBuffer);

        // World
        postProcessMaterial.SetTexture("_WorldTexture", WorldTexture);
        postProcessMaterial.SetVector("_WorldTextureSize", new Vector3(WorldTexture.width, WorldTexture.height, WorldTexture.depth));

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
        postProcessMaterial.SetFloat("_WaterLevel", waterLevel);
        postProcessMaterial.SetFloat("_WaterDensity", waterDensity);
        postProcessMaterial.SetVector("_WaterColor", waterColor);

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
