using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GetPoint : MonoBehaviour
{
    [SerializeField] List<GameObject> pointCollider;
    [SerializeField] WorldGenerator generator;
    [SerializeField] float colliderPrecision = 0.2f;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] int sizeXY;

    float score = 0;
    // Récupérer size depuis worldGenerator

    private Texture3D worldTexture => generator.WorldTexture;

    void Update()
    {
        foreach (GameObject obj in pointCollider)
        {
            updateScore(GetColliderPoint(obj));
        }
    }

    float GetColliderPoint(GameObject obj)
    {
        float points = 0;
        float volumeCollider = 0;
        BoxCollider boxCollider = obj.GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            Vector3 size = boxCollider.size;
            volumeCollider = (size.x / colliderPrecision) * (size.y / colliderPrecision) * (size.z / colliderPrecision);

            for (float x = -size.x / 2; x <= size.x / 2; x += colliderPrecision)
            {
                for (float y = -size.y / 2; y <= size.y / 2; y += colliderPrecision)
                {
                    for (float z = -size.z / 2; z <= size.z / 2; z += colliderPrecision)
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
        Color pixelColor = worldTexture.GetPixel(
            (int)coordinates.x % sizeXY,
            (int)coordinates.y % sizeXY,
            (int)coordinates.z % sizeXY);
        return pixelColor != Color.clear;
    }

    void updateScore(float points)
    {
        score += points;
        scoreText.text = "Score: " + score;
    }
}
