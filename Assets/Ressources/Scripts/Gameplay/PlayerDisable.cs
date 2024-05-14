using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class PlayerDisable : MonoBehaviour
{
    [SerializeField] GameObject PlaneMesh;
    [SerializeField] GameObject Explosion;
    private bool explosion;
    private Transform PlaneTransform;

    void Awake()
    {
        if (PlaneMesh != null && PlaneMesh.transform.parent != null)
        {
            PlaneTransform = PlaneMesh.transform.parent;
        }
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<ExplosionEvent>(PlaneExplosion);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<ExplosionEvent>(PlaneExplosion);
    }

    void PlaneExplosion(ExplosionEvent e)
    {
        explosion = true;
        Explosion.transform.parent = null;
        Explosion.SetActive(true);
    }

    void OnDisable()
    {
        if (PlaneMesh != null && !explosion)
        {
            PlaneMesh.transform.parent = null;
        }
        UnsubscribeEvents();
    }

    void OnEnable()
    {
        if (PlaneMesh != null && PlaneTransform != null)
        {
            PlaneMesh.transform.parent = PlaneTransform;
        }
        SubscribeEvents();
    }
}
