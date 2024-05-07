using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TestTools;

public class DepthOfFieldPostProcess : PostProcessBase
{
    [Header("General parameters")]
    [SerializeField] private Material postProcessMaterial;
    [SerializeField] private GBuffer gBuffer;
    [SerializeField] private float blurStart;
    [SerializeField] private float blurEnd;
    [SerializeField] private float blurScale;

    public void SetUniforms()
    {
        if (gBuffer.Initialized)
        {
            postProcessMaterial.SetTexture("_DepthTexture", gBuffer.DepthBuffer);
        }
        postProcessMaterial.SetFloat("_BlurStart", blurStart);
        postProcessMaterial.SetFloat("_BlurEnd", blurEnd);
        postProcessMaterial.SetFloat("_BlurScale", blurScale);
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
