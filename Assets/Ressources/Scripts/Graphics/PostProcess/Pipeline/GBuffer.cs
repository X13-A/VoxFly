using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GBuffer : MonoBehaviour, IEventHandler
{
    private int width;
    private int height;

    [SerializeField] private float voxelViewDistance;
    public float VoxelViewDistance => voxelViewDistance;

    public RenderTexture PositionBuffer { get; private set; } // The texture to write the positions
    public RenderTexture NormalBuffer { get; private set; } // The texture to write the normals
    public RenderTexture DepthBuffer { get; private set; } //  The texture to write the depth
    public RenderTexture BlockBuffer { get; private set; } // The texture to write block ID's

    [SerializeField] private ComputeShader shader;
    

    public bool Initialized { get; private set; }
    private int kernelHandle;
    private WorldGenerator generator;

    private void OnWorldGenerated(WorldGeneratedEvent e)
    {
        generator = e.generator;
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<StartPostProcessingEvent>(UpdateGBuffer);
        EventManager.Instance.AddListener<WorldGeneratedEvent>(OnWorldGenerated);
        EventManager.Instance.AddListener<ScreenResolutionChangedEvent>(HandleNewScreenResolution);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<StartPostProcessingEvent>(UpdateGBuffer);
        EventManager.Instance.RemoveListener<WorldGeneratedEvent>(OnWorldGenerated);
        EventManager.Instance.RemoveListener<ScreenResolutionChangedEvent>(HandleNewScreenResolution);
    }

    void OnEnable()
    {
        //Application.targetFrameRate = 60;
        SubscribeEvents();
    }

    private void OnDisable()
    {
        ReleaseBuffers();
        UnsubscribeEvents();
        Initialized = false;
    }

    void HandleNewScreenResolution(ScreenResolutionChangedEvent e)
    {
        width = e.gBufferWidth;
        height = e.gBufferHeight;
        Debug.Log($"Changed screen resolution to {width}x{height}");
        Setup();
    }

    void Setup()
    {        
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
        NormalBuffer.filterMode = FilterMode.Point; // Could act as free smoothing with bilinear
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
        shader.SetTexture(kernelHandle, "_BrickMapTexture", generator.BrickMapTexture);
        shader.SetInt("_BrickSize", generator.BrickSize);
        shader.SetInts("_GBufferSize", new int[] { width, height });
        shader.SetInts("_WorldTextureSize", new int[] { generator.WorldTexture.width, generator.WorldTexture.height, generator.WorldTexture.depth });
        shader.SetInts("_BrickMapTextureSize", new int[] { generator.BrickMapTexture.width, generator.BrickMapTexture.height, generator.BrickMapTexture.depth });
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
        shader.SetFloats("_CameraPos", new float[] { Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z });
        shader.SetMatrix("_InvProjectionMatrix", Camera.main.projectionMatrix.inverse);
        shader.SetMatrix("_InvViewMatrix", Camera.main.worldToCameraMatrix.inverse);
        shader.SetTextureFromGlobal(kernelHandle, "_UnityDepthTexture", "_CameraDepthTexture");
        shader.SetInts("_UnityBufferSize", new int[] { Screen.width, Screen.height });
        shader.Dispatch(kernelHandle, width / 8, height / 8, 1);
    }

    public void UpdateGBuffer(StartPostProcessingEvent e)
    {
        if (Camera.main == null)
        {
            Debug.Log("No camera active");
            return;
        }
        if (ScreenManager.Instance == null)
        {
            Debug.Log("Waiting for ScreenManager");
            return;
        }
        if (generator == null)
        {
            Debug.Log("Waiting for Generator");
            return;
        }
        if (!Initialized)
        {
            EventManager.Instance.Raise(new GBufferReadyForInitEvent());
        }
        if (Initialized)
        {
            Compute();
        }
    }
}
