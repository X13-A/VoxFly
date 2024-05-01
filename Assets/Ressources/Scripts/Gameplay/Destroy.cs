using Palmmedia.ReportGenerator.Core;
using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour, IEventHandler
{
    [SerializeField] List<GameObject> colliders;
    [SerializeField] WorldGenerator generator;
    [SerializeField] float pixelDetectionPrecision = 1;

    bool isGenerated = false;
    private int sizeY => generator.Size.y;
    private Texture3D worldTexture;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<WorldGeneratedEvent>(Generated);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<WorldGeneratedEvent>(Generated);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void Generated(WorldGeneratedEvent e)
    {
        worldTexture = generator.WorldTexture;
        isGenerated = true;
    }

    void Update()
    {
        if (isGenerated)
        {
            foreach (GameObject obj in colliders)
            {
                if (IsInTexture(obj))
                {
                    EventManager.Instance.Raise(new GameOverEvent());
                }
            }
        }
    }

    bool IsInTexture(GameObject obj)
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
