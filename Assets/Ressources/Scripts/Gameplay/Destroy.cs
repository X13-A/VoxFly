using Palmmedia.ReportGenerator.Core;
using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour, IEventHandler
{
    [SerializeField] private List<GameObject> colliders;
    [SerializeField] private float pixelDetectionPrecision = 1;

    bool isGenerated = false;
    private int sizeY => generator.Size.y;    
    private WorldGenerator generator;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GiveWorldGeneratorEvent>(OnWorldGenerated);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GiveWorldGeneratorEvent>(OnWorldGenerated);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void OnWorldGenerated(GiveWorldGeneratorEvent e)
    {
        generator = e.generator;
        isGenerated = true;
    }

    void Update()
    {
        if (isGenerated)
        {
            foreach (GameObject obj in colliders)
            {
                if (CollidesWithTerrain(obj))
                {
                    EventManager.Instance.Raise(new DestroyEvent());
                    break;
                }
            }
        }
    }

    bool CollidesWithTerrain(GameObject obj)
    {
        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            Vector3 size = boxCollider.size;
            Vector3 center = boxCollider.center;
            center = obj.transform.TransformPoint(center);

            if (center.y < 0 || center.y >= sizeY) return false;

            for (float x = -size.x / 2; x <= size.x / 2; x += pixelDetectionPrecision)
            {
                for (float y = -size.y / 2; y <= size.y / 2; y += pixelDetectionPrecision)
                {
                    for (float z = -size.z / 2; z <= size.z / 2; z += pixelDetectionPrecision)
                    {
                        Vector3 worldPoint = obj.transform.TransformPoint(x, y, z);
                        int a = generator.SampleWorld(worldPoint);

                        if (a > 0) return true;
                    }
                }
            }

        }

        return false;
    }
}
