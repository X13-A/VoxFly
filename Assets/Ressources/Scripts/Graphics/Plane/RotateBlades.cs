using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class RotateBlades : MonoBehaviour
{
    [SerializeField] private Transform blades;
    private plane planeCommands;
    [SerializeField] private float speedMultiplier = 2000;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaneInitializedEvent>(AttachPlane);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaneInitializedEvent>(AttachPlane);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }
    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void AttachPlane(PlaneInitializedEvent e)
    {
        planeCommands = e.plane;
    }

    void Update()
    {
        if (planeCommands == null)
        {
            blades.rotation *= Quaternion.Euler(0, 0, speedMultiplier * Time.deltaTime);
        }
        else
        {
            blades.rotation *= Quaternion.Euler(0, 0, (0.1f + planeCommands.Throttle) * speedMultiplier * Time.deltaTime); 
        }
    }
}
