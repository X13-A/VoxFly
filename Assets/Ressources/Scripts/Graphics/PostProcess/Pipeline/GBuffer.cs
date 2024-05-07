using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GBuffer : MonoBehaviour, IEventHandler
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float voxelViewDistance;
    public float VoxelViewDistance => voxelViewDistance;

    public RenderTexture PositionBuffer { get; private set; } // The texture to write the positions
    public RenderTexture NormalBuffer { get; private set; } // The texture to write the normals
    public RenderTexture DepthBuffer { get; private set; } //  The texture to write the depth
    public RenderTexture BlockBuffer { get; private set; } // The texture to write block ID's

    [SerializeField] private WorldGenerator generator;
    [SerializeField] private ComputeShader shader;
    [SerializeField] private Camera cam;

    public bool Initialized { get; private set; }
    private int kernelHandle;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<StartPostProcessingEvent>(UpdateGBuffer);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<StartPostProcessingEvent>(UpdateGBuffer);
    }

    void OnEnable()
    {
        //Application.targetFrameRate = 60;
        Setup();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        ReleaseBuffers();
        UnsubscribeEvents();
        Initialized = false;
    }

    void Setup()
    {
        if (!generator.WorldGenerated) return;
        
        kernelHandle = shader.FindKernel("CSMain");
        ReleaseBuffers();
        InitializeBuffers();
    }

    void InitializeBuffers()
    {
        PositionBuffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        PositionBuffer.enableRandomWrite = true;
        PositionBuffer.filterMode = FilterMode.Point;
        PositionBuffer.Create();

        NormalBuffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        NormalBuffer.enableRandomWrite = true;
        NormalBuffer.filterMode = FilterMode.Bilinear; // Can act as free anti aliasing
        NormalBuffer.Create();

        DepthBuffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        DepthBuffer.enableRandomWrite = true;
        DepthBuffer.filterMode = FilterMode.Point;
        DepthBuffer.Create();

        BlockBuffer = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
        BlockBuffer.enableRandomWrite = true;
        BlockBuffer.filterMode = FilterMode.Point;
        BlockBuffer.Create();

        shader.SetTexture(kernelHandle, "_PositionBuffer", PositionBuffer);
        shader.SetTexture(kernelHandle, "_NormalBuffer", NormalBuffer);
        shader.SetTexture(kernelHandle, "_DepthBuffer", DepthBuffer);
        shader.SetTexture(kernelHandle, "_BlocksBuffer", BlockBuffer);
        shader.SetTexture(kernelHandle, "_WorldTexture", generator.WorldTexture);
        shader.SetInts("_GBufferSize", new int[] { width, height });
        shader.SetInts("_WorldTextureSize", new int[] { generator.WorldTexture.width, generator.WorldTexture.height, generator.WorldTexture.depth });
        Initialized = true;
        EventManager.Instance.Raise(new GBufferInitializedEvent { gbuffer = this });
    }

    public void ReleaseBuffers()
    {
        if (PositionBuffer) PositionBuffer.Release();
        if (DepthBuffer) DepthBuffer.Release();
        if (BlockBuffer) BlockBuffer.Release();
        if (NormalBuffer) NormalBuffer.Release();
    }

    public void Compute()
    {
        foreach (Camera camera in Camera.allCameras)
        {
            camera.depthTextureMode |= DepthTextureMode.Depth;
        }
        if (Shader.GetGlobalTexture("_CameraDepthTexture") == null)
        {
            // Wait for Depth Texture
            return;
        }

        shader.SetFloat("_VoxelRenderDistance", voxelViewDistance);
        shader.SetFloats("_CameraPos", new float[] { cam.transform.position.x, cam.transform.position.y, cam.transform.position.z });
        shader.SetMatrix("_InvProjectionMatrix", cam.projectionMatrix.inverse);
        shader.SetMatrix("_InvViewMatrix", cam.worldToCameraMatrix.inverse);
        shader.SetTextureFromGlobal(kernelHandle, "_UnityDepthTexture", "_CameraDepthTexture");
        shader.SetInts("_UnityBufferSize", new int[] { Screen.width, Screen.height });
        
        shader.Dispatch(kernelHandle, (width + width % 8) / 8, (height + height % 8) / 8, 1);
    }

    public void UpdateGBuffer(StartPostProcessingEvent e)
    {
        if (cam == null)
        {
            Debug.Log("No camera active");
            return;
        }
        if (!Initialized)
        {
            Setup();
        }
        if (Initialized)
        {
            Compute();
        }
    }
}
