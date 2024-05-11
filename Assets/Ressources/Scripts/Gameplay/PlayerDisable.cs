using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDisable : MonoBehaviour
{
    [SerializeField] GameObject PlaneMesh;
    private Transform PlaneTransform;

    void Awake()
    {
        if (PlaneMesh != null && PlaneMesh.transform.parent != null)
        {
            PlaneTransform = PlaneMesh.transform.parent;
        }
    }

    void OnDisable()
    {
        if (PlaneMesh != null)
        {
            PlaneMesh.transform.parent = null;
        }
    }

    void OnEnable()
    {
        if (PlaneMesh != null && PlaneTransform != null)
        {
            PlaneMesh.transform.parent = PlaneTransform;
        }
    }
}
