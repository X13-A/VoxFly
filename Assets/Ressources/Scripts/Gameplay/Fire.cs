using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

public class Fire : MonoBehaviour, IEventHandler
{
    [SerializeField] private List<GameObject> fireLevel;

    bool flag = false;

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
            case < 8:
                break;

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
         if (flag) return;

        if (fireNumber == 0)
        {
            fireLevel[fireNumber].SetActive(true);
            return;
        }

        if (fireNumber == fireLevel.Count - 1) return;

        flag = true;

        GameObject currentFire = fireLevel[fireNumber];
        GameObject previousFire = fireLevel[fireNumber - 1];

        StartCoroutine(FireDelay(previousFire, currentFire, 2f));
    }

    IEnumerator FireDelay(GameObject oldFire, GameObject newFire, float delay)
    {
        newFire.SetActive(true);
        yield return new WaitForSeconds(delay);
        oldFire.SetActive(false);
        flag = false;
    }
}
