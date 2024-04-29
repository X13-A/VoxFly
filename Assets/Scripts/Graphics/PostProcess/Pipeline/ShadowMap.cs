using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;

public class ShadowMap : MonoBehaviour
{
    [SerializeField] private WorldGenerator generator;
    [SerializeField] private new Transform light;
    [SerializeField] private Transform lightForwardTransform;
    [SerializeField] private Transform lightUpTransform;
    [SerializeField] private Transform lightRightTransform;
    [SerializeField] private Camera cam;

    [SerializeField] private Transform shadowMapPlaneDEBUG;
    [Header("Light projection params")]
    [SerializeField] private float mapWidth = 50f;
    [SerializeField] private float mapHeight = 50f;
    [SerializeField] private float mapDistance = 100f;
    [SerializeField] private float farPlane = 500f;

    public float MapWidth => mapWidth;
    public float MapHeight => mapHeight;
    public float FarPlane => farPlane;

    public Vector3 Origin { get; private set; }

    public Vector3 LightDir => lightForwardTransform.forward;
    public Vector3 LightRight => lightRightTransform.forward;
    public Vector3 LightUp => lightUpTransform.forward;

    [Header("Computing")]

    [SerializeField] private ComputeShader shadowMapCompute;
    public RenderTexture ShadowMapRenderTexture { get; private set; }

    [SerializeField] private int textureWidth = 1920;
    [SerializeField] private int textureHeight = 1920;
    public int TextureWidth => textureWidth;
    public int TextureHeight => textureHeight;
    private int mapKernel;

    private bool initialized;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        if (lightForwardTransform == null) return;
        if (lightUpTransform == null) return;
        if (lightRightTransform== null) return;
        
        UnityEngine.Object.Destroy(ShadowMapRenderTexture);

        mapKernel = shadowMapCompute.FindKernel("CSMain");

        // Create Render Texture
        ShadowMapRenderTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGBFloat);
        ShadowMapRenderTexture.enableRandomWrite = true;
        ShadowMapRenderTexture.filterMode = FilterMode.Point;
        ShadowMapRenderTexture.Create();
        shadowMapCompute.SetTexture(mapKernel, "_ShadowMap", ShadowMapRenderTexture);

        initialized = true;
    }

    private void ComputeShadowMapOrigin()
    {
        Origin = new Vector3(Mathf.Round(cam.transform.position.x), Mathf.Round(cam.transform.position.y), Mathf.Round(cam.transform.position.z));
        Origin += new Vector3(LightDir.x, LightDir.y, LightDir.z) * -mapDistance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Setup();
        }

        if (!initialized) return;
        if (generator.WorldTexture == null) return;

        shadowMapCompute.SetFloats("_ShadowMapCoverage", new float[] { mapWidth, mapHeight });
        shadowMapCompute.SetInts("_ShadowMapResolution", new int[] { textureWidth, textureHeight });

        ComputeShadowMapOrigin();
        light.position = Origin;
        shadowMapPlaneDEBUG.localScale = new Vector3(mapWidth, 0.25f, mapHeight);
        shadowMapCompute.SetFloats("_StartPos", new float[] { Origin.x, Origin.y, Origin.z });
        shadowMapCompute.SetFloats("_LightDir", new float[] { LightDir.x, LightDir.y, LightDir.z });
        shadowMapCompute.SetFloats("_LightRight", new float[] { lightRightTransform.forward.x, lightRightTransform.forward.y, lightRightTransform.forward.z });
        shadowMapCompute.SetFloats("_LightUp", new float[] { lightUpTransform.forward.x, lightUpTransform.forward.y, lightUpTransform.forward.z });
        shadowMapCompute.SetFloat("_FarPlane", farPlane);

        shadowMapCompute.SetTexture(mapKernel, "_WorldTexture", generator.WorldTexture);
        shadowMapCompute.SetInts("_WorldTextureSize", new int[] { generator.WorldTexture.width, generator.WorldTexture.height, generator.WorldTexture.depth });

        shadowMapCompute.Dispatch(mapKernel, textureWidth / 8, textureHeight / 8, 1);
    }
}
