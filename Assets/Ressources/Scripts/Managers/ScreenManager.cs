using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class ScreenManager : Singleton<ScreenManager>, IEventHandler
{
    private int lastWidth;
    private int lastHeight;

    private int width;
    private int height;

    private float lastGBufferResolutionScale;
    [SerializeField][Range(0.01f, 1)] private float gBufferResolutionScale;
    public int GBufferHeight 
    {
        get 
        {
            int res = (int)(height * gBufferResolutionScale);
            res += (8 - res % 8) % 8;
            return res;
        }
    }
    public int GBufferWidth
    {
        get
        {
            int res = (int)(width * gBufferResolutionScale);
            res += (8 - res % 8) % 8;
            return res;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    void Init()
    {
        width = Screen.width;
        height = Screen.height;
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        EventManager.Instance.Raise(new ScreenResolutionChangedEvent { width = width, height = height, gBufferWidth = GBufferWidth, gBufferHeight = GBufferHeight });
    }

    void CheckForChanges()
    {
        width = Screen.width;
        height = Screen.height;

        // Check if the screen dimensions have changed
        if (width != lastWidth || height != lastHeight || gBufferResolutionScale != lastGBufferResolutionScale)
        {
            EventManager.Instance.Raise(new ScreenResolutionChangedEvent { width = width, height = height, gBufferWidth = GBufferWidth, gBufferHeight = GBufferHeight });
            lastWidth = width;
            lastHeight = height;
        }
        lastGBufferResolutionScale = gBufferResolutionScale;
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GBufferReadyForInitEvent>(EmitResolution);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GBufferReadyForInitEvent>(EmitResolution);
    }

    void EmitResolution(GBufferReadyForInitEvent e)
    {
        EventManager.Instance.Raise(new ScreenResolutionChangedEvent { width = width, height = height, gBufferWidth = GBufferWidth, gBufferHeight = GBufferHeight });
    }

    void Update()
    {
        CheckForChanges();
    }
}