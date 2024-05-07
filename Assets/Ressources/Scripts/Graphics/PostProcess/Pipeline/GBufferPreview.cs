using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SDD.Events;

public class GBufferPreview : MonoBehaviour, IEventHandler
{
    private GBuffer gBuffer;
    [SerializeField] private ShadowMap shadowMap;

    [SerializeField] private RawImage blockPreview;
    [SerializeField] private RawImage normalPreview;
    [SerializeField] private RawImage depthPreview;
    [SerializeField] private RawImage positionPreview;
    [SerializeField] private RawImage shadowMapPreview;
    [SerializeField] private TextMeshProUGUI FPS;

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
        EventManager.Instance.RemoveListener<GBufferInitializedEvent>(AttachGBuffer);
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    public void Update()
    {
        if (gBuffer == null) return;

        FPS.text = $"FPS: { (int) (100 / Time.smoothDeltaTime) / 100.0f}";
        blockPreview.texture = gBuffer.BlockBuffer;
        normalPreview.texture = gBuffer.NormalBuffer;
        depthPreview.texture = gBuffer.DepthBuffer;
        positionPreview.texture = gBuffer.PositionBuffer;
        shadowMapPreview.texture = shadowMap.ShadowMapRenderTexture;
    }
}
