using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class SunBurn : MonoBehaviour
{
    [SerializeField] private Transform directionalLight;
    [SerializeField] private WorldGenerator generator;
    [SerializeField] private CloudsPostProcess cloudsPostProcess;

    private struct RayBoxInfo
    {
        public float dstToBox;
        public float dstInsideBox;

        public RayBoxInfo(float dstToBox, float dstInsideBox)
        {
            this.dstToBox = dstToBox;
            this.dstInsideBox = dstInsideBox;
        }
    }

    RayBoxInfo rayBoxDst(Vector3 rayOrigin, Vector3 rayDir)
    {
        Vector3 invRayDir =  new Vector3(1f / rayDir.x, 1f / rayDir.y, 1f / rayDir.z);

        // Calculate ray intersections with box
        Vector3 t0 = (cloudsPostProcess.BoundsMin - rayOrigin);
        t0.Scale(invRayDir);
        Vector3 t1 = (cloudsPostProcess.BoundsMax - rayOrigin);
        t1.Scale(invRayDir);
        Vector3 tmin = Vector3.Min(t0, t1);
        Vector3 tmax = Vector3.Max(t0, t1);

        // Calculate distances
        float dstA = Mathf.Max(Mathf.Max(tmin.x, tmin.y), tmin.z); // A is the closest point
        float dstB = Mathf.Min(tmax.x, Mathf.Min(tmax.y, tmax.z)); // B is the furthest point

        float dstToBox = Mathf.Max(0, dstA);
        float dstInsideBox = Mathf.Max(0, dstB - dstToBox);
        return new RayBoxInfo(dstToBox, dstInsideBox);
    }

    private float CloudCoverage(Vector3 pos, Vector3 lightDir)
    {
        RayBoxInfo rayBoxInfo = rayBoxDst(pos, lightDir);
        float dstToBox = rayBoxInfo.dstToBox;
        float dstInsideBox = rayBoxInfo.dstInsideBox;

        Vector3 startPos = pos + lightDir * dstToBox;
        Vector3 rayPos = startPos;

        int shadowSteps = 20;
        float stepSize = dstInsideBox / shadowSteps;

        float shadow = 0;
        for (int i = 0; i < shadowSteps; i++)
        {
            rayPos = startPos + lightDir * stepSize * i;
            float density = cloudsPostProcess.SampleCoverage(rayPos).x * stepSize;
            shadow += density;
        }
        return shadow;
    }

    void Update()
    {
        if (!generator.WorldGenerated) return;

        float maxCoverage = 75;
        float resultIntensity = maxCoverage - Mathf.Clamp(CloudCoverage(transform.position, -directionalLight.forward), 0, maxCoverage);
        resultIntensity /= maxCoverage;
        bool result = !generator.RayCastWorld(transform.position, -directionalLight.forward).hit;

        if (result)
        {
            EventManager.Instance.Raise(new PlaneIsInShadowEvent() { eIsInShadow = false, eRayRate = resultIntensity });
        }
        else
        {
            EventManager.Instance.Raise(new PlaneIsInShadowEvent() { eIsInShadow = true, eRayRate = resultIntensity });
        }
    }
}