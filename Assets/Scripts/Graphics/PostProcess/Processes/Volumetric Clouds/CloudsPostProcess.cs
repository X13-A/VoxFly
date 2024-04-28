using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using UnityEngine;

public class CloudsPostProcess : PostProcessBase
{
    [Header("General parameters")]
    [SerializeField] private Material postProcessMaterial;
    [SerializeField] private CloudsNoiseGenerator noiseGenerator;
    [SerializeField] private GBuffer gBuffer;

    [Header("Shape parameters")]
    [SerializeField] private Transform container;
    [SerializeField] private Texture2D heightMap;
    [SerializeField] private float heightScale = 1000;
    [SerializeField] private float heightVariation = 200;
    [SerializeField] private Vector2 heightMapSpeed = new Vector2();

    [SerializeField] private Texture2D coverageMap;
    [SerializeField] private float coverageScale = 1000;
    [SerializeField] private Vector2 coverageOffset = new Vector2();
    [SerializeField] private Vector2 coverageSpeed= new Vector2();
    [SerializeField] [Range(0, 1)] private float coverage = 1;

    [Header("Lighting paramaters")]
    [SerializeField] private Vector3 phaseParams = new Vector3(0.8f, 0, 0.7f); // Y parameter useless right now
    [SerializeField] private int lightSteps = 3;
    [SerializeField] private float globalBrightness = 1;
    [SerializeField] private float globalDensity = 0.25f;
    [SerializeField] private float sunlightAbsorption = 0.25f;

    [Header("Sample parameters")]
    [SerializeField] private float stepSizeDistanceScale = 0.0001f;
    [SerializeField] private float minStepSize = 20;
    [SerializeField] private float maxStepSize = 50;
    [SerializeField] private Texture2D blueNoise;
    [SerializeField] private float offsetNoiseIntensity = 150;

    [Header("Shadows parameters")]
    [SerializeField] private float shadowIntensity = 2;
    [SerializeField] private uint shadowSteps = 3;
    [SerializeField] private float shadowDist = 10000;

    private float GetStepSize()
    {
        float containerDist = Mathf.Abs(container.transform.position.y - Camera.current.transform.position.y);
        containerDist -= container.localScale.y / 2;
        containerDist = Mathf.Max(containerDist, 0);
        float computedStepSize = Mathf.Min(minStepSize + minStepSize * (containerDist * stepSizeDistanceScale), maxStepSize);
        if (computedStepSize < 0.1f)
        {
            computedStepSize = 0.1f;
        }
        return computedStepSize;
    }

    public void SetUniforms()
    {
        if (gBuffer.Initialized)
        {
            postProcessMaterial.SetTexture("_DepthTexture", gBuffer.DepthBuffer);
            postProcessMaterial.SetTexture("_PositionTexture", gBuffer.PositionBuffer);
        }

        // Noise params
        postProcessMaterial.SetTexture("_BlueNoise", blueNoise);
        postProcessMaterial.SetFloat("_OffsetNoiseIntensity", offsetNoiseIntensity);

        // Sample params
        float computedStepSize = GetStepSize();
        postProcessMaterial.SetFloat("_StepSize", computedStepSize);
        
        // Shape params
        Vector3 halfExtents = Vector3.one * 0.5f; // TODO: Find min and max here
        halfExtents.Scale(container.localScale);
        Vector3 corner1 = container.position - container.localScale/2;
        Vector3 corner2 = container.position + container.localScale/2;

        postProcessMaterial.SetVector("_BoundsMin", corner1);
        postProcessMaterial.SetVector("_BoundsMax", corner2);
        postProcessMaterial.SetTexture("_HeightMap", heightMap);
        postProcessMaterial.SetFloat("_HeightScale", heightScale);
        postProcessMaterial.SetVector("_HeightMapSpeed", heightMapSpeed);
        postProcessMaterial.SetFloat("_HeightVariation", heightVariation);
        postProcessMaterial.SetTexture("_CoverageMap", coverageMap);
        postProcessMaterial.SetFloat("_CoverageScale", coverageScale);
        postProcessMaterial.SetVector("_CoverageOffset", coverageOffset);
        postProcessMaterial.SetVector("_CoverageSpeed", coverageSpeed);
        postProcessMaterial.SetFloat("_Coverage", coverage);

        // Lighting params
        postProcessMaterial.SetInteger("_LightSteps", lightSteps);
        postProcessMaterial.SetFloat("_GlobalBrightness", globalBrightness);
        postProcessMaterial.SetFloat("_GlobalDensity", globalDensity);
        postProcessMaterial.SetFloat("_SunLightAbsorption", sunlightAbsorption);
        postProcessMaterial.SetVector("_PhaseParams", phaseParams);
        postProcessMaterial.SetFloat("_ShadowIntensity", shadowIntensity);
        postProcessMaterial.SetInteger("_ShadowSteps", (int)shadowSteps);
        postProcessMaterial.SetFloat("_ShadowDist", shadowDist);
    }
    public override void Apply(RenderTexture source, RenderTexture dest)
    {
        if (postProcessMaterial != null && Camera.current != null)
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
