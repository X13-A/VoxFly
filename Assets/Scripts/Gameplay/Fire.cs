using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour, IEventHandler
{
    [SerializeField] private GameObject fire;
    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaneStateEvent>(StateBurningRate);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaneStateEvent>(StateBurningRate);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void Start()
    {
        fire.SetActive(false);
    }

    void StateBurningRate(PlaneStateEvent e)
    {
        switch (e.eBurningRate)
        { 
            case < 25:
                fire.SetActive(false);
                fire.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                break;

            case < 50:
                fire.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                break;
                
            case < 75:
                fire.transform.localScale = new Vector3(1f, 1f, 1f);
                break;

            case < 100:
                fire.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                break;

            default:
                break;
        }
    }
}
