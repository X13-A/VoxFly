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
    [SerializeField] private float dstLimit = 1000;

    private struct RayWorldInfo
    {
        public float dstToWorld;
        public float dstInsideWorld;

        public RayWorldInfo(float dstToWorld, float dstInsideWorld)
        {
            this.dstToWorld = dstToWorld;
            this.dstInsideWorld = dstInsideWorld;
        }
    }

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

    private float DstToPlane(Vector3 rayOrigin, Vector3 rayDir, float planeY)
    {
        // Check if the plane is parallel to the ray
        if (rayDir.y == 0)
        {
            return -1.0f;
        }

        float t = (planeY - rayOrigin.y) / rayDir.y;

        // Check if the plane is behind the ray's origin
        if (t < 0)
        {
            return -1.0f;
        }

        return t;
    }

    private RayWorldInfo RayWorldHit(Vector3 pos, Vector3 dir)
    {
        float dstTop = DstToPlane(pos, dir, generator.WorldTexture.height);
        float dstBottom = DstToPlane(pos, dir, 0);

        float dstToWorld;
        float dstInsideWorld;

        // If inside the world
        if (generator.IsInWorld(pos))
        {
            dstToWorld = 0;
            // Check if direction is parallel to the planes
            if (dir.y == 0) return new RayWorldInfo(dstToWorld, float.PositiveInfinity);
            // Return dist inside world
            return new RayWorldInfo(dstToWorld, Mathf.Max(dstTop, dstBottom));
        }

        // If above the world
        if (pos.y > generator.WorldTexture.height)
        {
            // Check if looking at world
            if (dstTop < 0) return new RayWorldInfo(-1, -1);

            dstInsideWorld = dstBottom - dstTop;
            return new RayWorldInfo(dstTop, dstInsideWorld);
        }
        // If under the world
        else
        {
            // Check if looking at world
            if (dstBottom < 0) return new RayWorldInfo(-1, -1);

            dstInsideWorld = dstTop - dstBottom;
            return new RayWorldInfo(dstBottom, dstInsideWorld);
        }
    }

    private bool IsInShadow(Vector3 pos, Vector3 lightDir)
    {
        Vector3 startPos = pos;
        Vector3 rayDir = lightDir;

        RayWorldInfo rayWorldInfo = RayWorldHit(startPos, rayDir);
        float dstToWorld = rayWorldInfo.dstToWorld;
        float dstInsideWorld = rayWorldInfo.dstInsideWorld;

        // EXIT EARLY
        if (dstInsideWorld <= 0)
        {
            return false;
        }
        if (dstToWorld > 0)
        {
            startPos = startPos + rayDir * dstToWorld; // Start at intersection point
        }

        Vector3Int voxelIndex = new Vector3Int((int) Mathf.Round(startPos.x), (int) Mathf.Round(startPos.y), (int) Mathf.Round(startPos.z));

        Vector3Int step = new Vector3Int((int) Mathf.Sign(rayDir.x), (int) Mathf.Sign(rayDir.y), (int) Mathf.Sign(rayDir.z));
        Vector3 tMax = new Vector3();  // Distance to next voxel boundary
        Vector3 tDelta = new Vector3();  // How far we travel to cross a voxel

        // Calculate initial tMax and tDelta for each axis
        for (int i = 0; i < 3; i++)
        {
            if (rayDir[i] == 0)
            {
                tMax[i] = float.PositiveInfinity;
                tDelta[i] = float.PositiveInfinity;
            }
            else
            {
                float voxelBoundary = voxelIndex[i] + (step[i] > 0 ? 1 : 0);
                tMax[i] = (voxelBoundary - startPos[i]) / rayDir[i];
                tDelta[i] = Mathf.Abs(1.0f / rayDir[i]);
            }
        }

        float dstLimit = Mathf.Min(this.dstLimit - dstToWorld, dstInsideWorld);
        float dstTravelled = 0;
        int loopCount = 0;
        int hardLoopLimit = (int)dstLimit * 2; // Hack that prevents convergence caused by precision issues or some dark mathematical magic

        while (loopCount < hardLoopLimit && dstTravelled < dstLimit)
        {
            // Check the position for a voxel
            Vector3 rayPos = startPos + rayDir * (dstTravelled + 0.001f);

            Vector3Int sampledVoxelIndex = new Vector3Int((int)Mathf.Round(rayPos.x), (int)Mathf.Round(rayPos.y), (int)Mathf.Round(rayPos.z));
            int blockID = generator.SampleWorld(rayPos);

            // Return the voxel
            if (blockID != -1)
            {
                return true;
            }

            // Move to the next voxel
            if (tMax.x < tMax.y && tMax.x < tMax.z)
            {
                dstTravelled = tMax.x;
                tMax.x += tDelta.x;
                voxelIndex.x += step.x;
            }
            else if (tMax.y < tMax.z)
            {
                dstTravelled = tMax.y;
                tMax.y += tDelta.y;
                voxelIndex.y += step.y;
            }
            else
            {
                dstTravelled = tMax.z;
                tMax.z += tDelta.z;
                voxelIndex.z += step.z;
            }
            loopCount++;
        }
        return false;
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
        float maxCoverage = 75;
        float resultIntensity = maxCoverage - Mathf.Clamp(CloudCoverage(transform.position, -directionalLight.forward), 0, maxCoverage);
        resultIntensity /= maxCoverage;
        bool result = !IsInShadow(transform.position, -directionalLight.forward);

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
