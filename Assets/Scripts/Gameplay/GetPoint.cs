using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetPoint : MonoBehaviour
{
    [SerializeField] List<GameObject> colliders;
    [SerializeField] WorldGenerator generator;
    [SerializeField] float pixelDetectionPrecision = 1;
    [SerializeField] TextMeshProUGUI scoreText;

    float score = 0;
    private int sizeX => generator.Size.x;
    private int sizeY => generator.Size.y;
    private int sizeZ => generator.Size.z;
    private Texture3D worldTexture => generator.WorldTexture;

    private void Start()
    {
        updateScore(0);
    }

    void Update()
    {
        foreach (GameObject obj in colliders)
        {
            float points = GetColliderPoint(obj);
            if (points > 0) updateScore(points);
        }
    }

    float GetColliderPoint(GameObject obj)
    {
        float points = 0;
        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            Vector3 size = boxCollider.size;
            Vector3 center = boxCollider.center;
            center = obj.transform.TransformPoint(center);

            if (center.y >= sizeY) return 0;

            for (float x = -size.x / 2; x <= size.x / 2; x += pixelDetectionPrecision)
            {
                for (float y = -size.y / 2; y <= size.y / 2; y += pixelDetectionPrecision)
                {
                    for (float z = -size.z / 2; z <= size.z / 2; z += pixelDetectionPrecision)
                    {
                        Vector3 point = new Vector3(x, y, z);
                        Vector3 worldPoint = obj.transform.TransformPoint(point);
                        if (IsInTexture(worldPoint))
                        {
                            points++;
                        }
                    }
                }
            }
        }

        return points;
    }

    bool IsInTexture(Vector3 coordinates)
    {
        int x = ((int)coordinates.x) % sizeX;
        int y = (int)coordinates.y;
        int z = ((int)coordinates.z) % sizeZ;

        if (x < 0) x += sizeX;
        if (z < 0) z += sizeZ;

        Color pixelColor = worldTexture.GetPixel(x, y, z);

        return pixelColor != Color.clear;
    }

    void updateScore(float points)
    {
        score += points;
        scoreText.text = "Score: " + score;
    }
}
