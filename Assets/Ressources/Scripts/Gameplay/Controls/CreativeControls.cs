using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class CreativeControls : MonoBehaviour, IEventHandler
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private Camera cam;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float mouseSensitivity = 100f;
    public float movementDamping = 0.1f;
    public float rotationDamping = 0.1f;

    [SerializeField] private List<GameObject> objectsToDisable;
    [SerializeField] private List<GameObject> objectsToEnable;

    private Vector3 movementVelocity;
    private Vector2 rotationVelocity;

    private bool active = false;

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
        EventManager.Instance.AddListener<GamePlayEvent>(HandlePlay);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GamePlayEvent>(HandlePlay);
    }

    private void HandlePlay(GamePlayEvent e)
    {
        if (ConfigManager.Instance.CurrentConfig.name == "Creative Mode")
        {
            SwitchToCreativeMode();
        }
    }

    private void SwitchToCreativeMode()
    {
        active = true;
        EventManager.Instance.Raise(new ToggleFlashlightVolumetricsEvent { value = false });
        EventManager.Instance.Raise(new SetCloudCoverageEvent { eValue = 0.8f });
        EventManager.Instance.Raise(new CreativeModeStartEvent { });
        cam.gameObject.SetActive(true);

        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in objectsToEnable)
        {
            obj.SetActive(true);
        }
    }

    private void Update()
    {
        if (!active) return;
        if (!GameManager.Instance.IsPlaying)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
        Application.targetFrameRate = 60; // Better for video recording, prevents lag
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 targetMovement = new Vector3(horizontalInput, 0, verticalInput);
        targetMovement = controller.transform.TransformDirection(targetMovement);

        if (Input.GetKey(KeyCode.LeftControl))
        {
            targetMovement *= 2;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            targetMovement.y += 1;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            targetMovement.y -= 1;
        }

        movementVelocity = Vector3.Lerp(movementVelocity, targetMovement * movementSpeed, movementDamping * Time.deltaTime);
        controller.Move(movementVelocity * Time.deltaTime);

        // Camera Rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotationVelocity.x = Mathf.Lerp(rotationVelocity.x, mouseX, rotationDamping * Time.deltaTime);
        rotationVelocity.y = Mathf.Lerp(rotationVelocity.y, mouseY, rotationDamping * Time.deltaTime);

        controller.transform.Rotate(Vector3.up * rotationVelocity.x);
        cam.transform.Rotate(Vector3.left * rotationVelocity.y);
    }
}
