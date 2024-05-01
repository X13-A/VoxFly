using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSound : MonoBehaviour, IEventHandler
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] float maxAudio = .5f;
    [SerializeField] float minAudio = .2f;

    float minThrust;
    float maxThrust;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaneStateEvent>(Thrusting);
        EventManager.Instance.AddListener<PlaneInformationEvent>(PlaneInformation);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaneStateEvent>(Thrusting);
        EventManager.Instance.RemoveListener<PlaneInformationEvent>(PlaneInformation);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void PlaneInformation(PlaneInformationEvent e)
    {
        minThrust = e.eMinThrust;
        maxThrust = e.eMaxThrust;

        Debug.Log(minThrust + " " + maxThrust);
    }

    void Thrusting(PlaneStateEvent e)
    {
        float thrust = e.eThrust;
    }
}
