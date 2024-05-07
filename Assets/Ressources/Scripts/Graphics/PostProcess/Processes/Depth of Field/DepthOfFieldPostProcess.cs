using SDD.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TestTools;

public class DepthOfFieldPostProcess : PostProcessBase, IEventHandler
{
    [Header("General parameters")]
    [SerializeField] private Material postProcessMaterial;
    [SerializeField] private float blurStart;
    [SerializeField] private float blurEnd;
    [SerializeField] private float blurScale;
    private GBuffer gBuffer;

    #region Events
    private void AttachGBuffer(GBufferInitializedEvent e)
    {
        gBuffer = e.gbuffer;
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GBufferInitializedEvent>(AttachGBuffer);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.AddListener<GBufferInitializedEvent>(AttachGBuffer);
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
        if (gBuffer != null && postProcessMaterial != null && Camera.current != null)
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
