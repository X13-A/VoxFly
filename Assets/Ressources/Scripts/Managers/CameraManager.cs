using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using Unity.VisualScripting;
using static UnityEngine.UI.CanvasScaler;
using System.Runtime.CompilerServices;

public enum CameraMode { FirstPerson, ThirdPerson }

public class CameraManager : MonoBehaviour, IEventHandler
{
    public static CameraManager m_Instance;
    public static CameraManager Instance { get { return m_Instance; } }

    [Header("First Person")]
    [SerializeField] private Camera firstPerson_Camera;
    [SerializeField] private GameObject blackScreen;
    [SerializeField] private WorldPostProcess worldPostProcess;
    private float initialVolumetricIntensity;

    [Header("Third Person")]
    [SerializeField] private Camera thirdPerson_Camera;
    [SerializeField] private GameObject thirdPerson_MouseFlightRig;
    
    private CameraMode cameraMode;

    void Awake()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Init()
    {
        initialVolumetricIntensity = worldPostProcess.playerLightVolumetricIntensity;
        if (Camera.main == firstPerson_Camera)
        {
            EventManager.Instance.Raise(new SwitchToFirstPersonEvent());
        }
        else if (Camera.main == thirdPerson_Camera)
        {
            EventManager.Instance.Raise(new SwitchToThirdPersonEvent());
        }
    }

    void OnEnable()
    {
        SubscribeEvents();
        Init();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<SwitchToThirdPersonEvent>(SwitchToThirdPerson);
        EventManager.Instance.AddListener<SwitchToFirstPersonEvent>(SwitchToFirstPerson);
        EventManager.Instance.AddListener<DestroyEvent>(HandleDestroy);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<SwitchToThirdPersonEvent>(SwitchToThirdPerson);
        EventManager.Instance.RemoveListener<SwitchToFirstPersonEvent>(SwitchToFirstPerson);
        EventManager.Instance.RemoveListener<DestroyEvent>(HandleDestroy);
    }

    public void SwitchToFirstPerson(SwitchToFirstPersonEvent e)
    {
        thirdPerson_Camera.gameObject.SetActive(false);
        thirdPerson_MouseFlightRig.SetActive(false);
        firstPerson_Camera.gameObject.SetActive(true);
        cameraMode = CameraMode.FirstPerson;
        worldPostProcess.playerLightVolumetricIntensity = 0;
    }

    public void SwitchToThirdPerson(SwitchToThirdPersonEvent e)
    {
        thirdPerson_Camera.gameObject.SetActive(true);
        thirdPerson_MouseFlightRig.SetActive(true);
        firstPerson_Camera.gameObject.SetActive(false);
        cameraMode = CameraMode.ThirdPerson;
        worldPostProcess.playerLightVolumetricIntensity = initialVolumetricIntensity;
    }

    public void HandleDestroy(DestroyEvent e)
    {
        if (cameraMode == CameraMode.FirstPerson)
        {
            // Prevents the camera from being disabled
            firstPerson_Camera.transform.SetParent(null);
            blackScreen.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (cameraMode == CameraMode.FirstPerson)
            {
                EventManager.Instance.Raise(new SwitchToThirdPersonEvent());
            }
            else if (cameraMode == CameraMode.ThirdPerson)
            {
                EventManager.Instance.Raise(new SwitchToFirstPersonEvent());
            }
        }
    }
}
