using Palmmedia.ReportGenerator.Core;
using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetPoint : MonoBehaviour, IEventHandler
{
    [SerializeField] private List<GameObject> colliders;
    [SerializeField] private float pixelDetectionPrecision = 1;
    [SerializeField] private TextMeshProUGUI scoreText;

    private bool isGenerated = false;
    private int sizeY => generator.Size.y;
    private WorldGenerator generator;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<WorldGeneratedEvent>(OnGenerated);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<WorldGeneratedEvent>(OnGenerated);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void OnGenerated(WorldGeneratedEvent e)
    {
        generator = e.generator;
        isGenerated = true;
    }

    void Start()
    {
    }

    void Update()
    {
        if (!isGenerated) return;
        
        foreach (GameObject obj in colliders)
        {
            int points = GetColliderPoint(obj);
            if (points > 0) UpdateScore(points);
        }
    }

    int GetColliderPoint(GameObject obj)
    {
        int points = 0;
        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            Vector3 size = boxCollider.size;
            Vector3 center = boxCollider.center;
            center = obj.transform.TransformPoint(center);

            if (center.y < 0 || center.y >= sizeY) return 0;

            for (float x = -size.x / 2; x <= size.x / 2; x += pixelDetectionPrecision)
            {
                for (float y = -size.y / 2; y <= size.y / 2; y += pixelDetectionPrecision)
                {
                    for (float z = -size.z / 2; z <= size.z / 2; z += pixelDetectionPrecision)
                    {
                        Vector3 worldPoint = obj.transform.TransformPoint(x, y, z);
                        int a = generator.SampleWorld(worldPoint);
                        if (a > 0)
                        {
                            points++;
                        }
                    }
                }
            }
        }

        return points;
    }

    void UpdateScore(int points)
    {
        GameManager.Instance.IncrementScore(points);
    }
}
