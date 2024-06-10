using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using Unity.VisualScripting;
using static UnityEngine.UI.CanvasScaler;
using System.Runtime.CompilerServices;

public enum CameraMode { FirstPerson, ThirdPerson }

public class CameraManager : Singleton<CameraManager>, IEventHandler
{

    [Header("First Person")]
    [SerializeField] private Camera firstPerson_Camera;

    [Header("Third Person")]
    [SerializeField] private Camera thirdPerson_Camera;
    [SerializeField] private GameObject thirdPerson_MouseFlightRig;
    
    private CameraMode cameraMode;
    private bool ready;

    void Init()
    {
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
        EventManager.Instance.AddListener<FinishTimerEvent>(HandleTimerFinished);
        EventManager.Instance.AddListener<DestroyEvent>(HandleDestroy);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<SwitchToThirdPersonEvent>(SwitchToThirdPerson);
        EventManager.Instance.RemoveListener<SwitchToFirstPersonEvent>(SwitchToFirstPerson);
        EventManager.Instance.RemoveListener<FinishTimerEvent>(HandleTimerFinished);
        EventManager.Instance.RemoveListener<DestroyEvent>(HandleDestroy);
    }

    private void SwitchToFirstPerson(SwitchToFirstPersonEvent e)
    {
        thirdPerson_Camera.gameObject.SetActive(false);
        thirdPerson_MouseFlightRig.SetActive(false);
        firstPerson_Camera.gameObject.SetActive(true);
        cameraMode = CameraMode.FirstPerson;
        EventManager.Instance.Raise(new ToggleFlashlightVolumetricsEvent { value = false });
    }

    private void SwitchToThirdPerson(SwitchToThirdPersonEvent e)
    {
        thirdPerson_Camera.gameObject.SetActive(true);
        thirdPerson_MouseFlightRig.SetActive(true);
        firstPerson_Camera.gameObject.SetActive(false);
        cameraMode = CameraMode.ThirdPerson;
        EventManager.Instance.Raise(new ToggleFlashlightVolumetricsEvent { value = true });
    }

    private void HandleTimerFinished(FinishTimerEvent e)
    {
        ready = true;
    }

    private void HandleDestroy(DestroyEvent e)
    {
        if (cameraMode == CameraMode.FirstPerson)
        {
            // Prevents the camera from being disabled
            firstPerson_Camera.transform.localPosition -= new Vector3(0, 0, 7);

            // Remove roll
            Vector3 currentRotation = firstPerson_Camera.transform.eulerAngles;
            float pitch = currentRotation.x;
            float yaw = currentRotation.y;
            firstPerson_Camera.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            firstPerson_Camera.transform.SetParent(null);
            //blackScreen.SetActive(true);
        }
    }

    private void Update()
    {
        if (ready && Input.GetKeyDown(KeyCode.C))
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
