using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour, IEventHandler
{
    [SerializeField] private List<GameObject> fireLevel;
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

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void Start()
    {
        foreach (var item in fireLevel)
        {
            item.SetActive(false);
        }
    }

    void StateBurningRate(PlaneStateEvent e)
    {
        switch (e.eBurningRate)
        { 
            case < 25:
                FireUpdate(0);
                break;

            case < 50:
                FireUpdate(1);
                break;
                
            case < 75:
                FireUpdate(2);
                break;

            case < 100:
                FireUpdate(3);
                break;

            default:
                break;
        }
    }

    void FireUpdate(int fireNumber)
    {
        for (int i = 0; i < fireLevel.Count; i++)
        {
            if (i == fireNumber)
            {
                fireLevel[i].SetActive(true);
            }
            else
            {
                fireLevel[i].SetActive(false);
            }
        }
    }
}
