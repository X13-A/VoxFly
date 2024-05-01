using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GBufferPreview : MonoBehaviour
{
    [SerializeField] private GBuffer gBuffer;
    [SerializeField] private ShadowMap shadowMap;

    [SerializeField] private RawImage blockPreview;
    [SerializeField] private RawImage normalPreview;
    [SerializeField] private RawImage depthPreview;
    [SerializeField] private RawImage positionPreview;
    [SerializeField] private RawImage shadowMapPreview;
    [SerializeField] private TextMeshProUGUI FPS;
    public void Update()
    {
        FPS.text = $"FPS: { (int) (100 / Time.smoothDeltaTime) / 100.0f}";
        blockPreview.texture = gBuffer.BlockBuffer;
        normalPreview.texture = gBuffer.NormalBuffer;
        depthPreview.texture = gBuffer.DepthBuffer;
        positionPreview.texture = gBuffer.PositionBuffer;
        shadowMapPreview.texture = shadowMap.ShadowMapRenderTexture;
    }
}
