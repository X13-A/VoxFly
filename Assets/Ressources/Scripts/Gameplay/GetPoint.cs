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

    private float scoreSoundDelay = 0.1f;
    private float scoreSoundTime;

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

        return points * 60;
    }

    void UpdateScore(int points)
    {
        if (points == 0) return;

        float pitchVariation = 0.5f;
        float pitch = Random.Range(1 - pitchVariation / 2f, 1 + pitchVariation / 2f);
        
        if (Time.time - scoreSoundTime > scoreSoundDelay)
        {
            scoreSoundTime = Time.time;
            EventManager.Instance.Raise(new PlaySoundEvent { eNameClip = "score1", eCanStack = true, eDestroyWhenFinished = true, ePitch = pitch, eVolumeMultiplier = 8f });
        }

        // Make a sound every "increment" points
        int increment = 500;
        int increments_before = GameManager.Instance.Score / increment;
        GameManager.Instance.IncrementScore((int) (points * Time.deltaTime));
        int increments_after = GameManager.Instance.Score / increment;
        if (increments_after > increments_before)
        {
            EventManager.Instance.Raise(new PlaySoundEvent { eNameClip = "score2", eCanStack = true, eDestroyWhenFinished = true, ePitch = pitch, eVolumeMultiplier = 0.3f });
        }
    }
}
