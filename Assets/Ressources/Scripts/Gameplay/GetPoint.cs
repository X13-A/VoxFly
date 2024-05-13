using Palmmedia.ReportGenerator.Core;
using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetPoint : MonoBehaviour, IEventHandler
{
    [SerializeField] private List<GameObject> colliders;
    [SerializeField] private float detectionSteps = 1;
    [SerializeField] private TextMeshProUGUI scoreText;

    private bool isGenerated = false;
    private int sizeY => generator.Size.y;
    private WorldGenerator generator;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GiveWorldGeneratorEvent>(OnGenerated);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GiveWorldGeneratorEvent>(OnGenerated);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void OnGenerated(GiveWorldGeneratorEvent e)
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

            for (float x = -size.x / 2; x <= size.x / 2; x += size.x / detectionSteps)
            {
                for (float y = -size.y / 2; y <= size.y / 2; y += size.y / detectionSteps)
                {
                    for (float z = -size.z / 2; z <= size.z / 2; z += size.z / detectionSteps)
                    {
                        // HACK: Prevent overflow
                        x = Mathf.Clamp(x, -size.x / 2, size.x / 2);
                        y = Mathf.Clamp(y, -size.y / 2, size.y / 2);
                        z = Mathf.Clamp(z, -size.z / 2, size.z / 2);

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
