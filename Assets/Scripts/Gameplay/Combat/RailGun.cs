using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RailGun : MonoBehaviour
{
    [SerializeField] private WorldGenerator generator;
    [SerializeField] private float cooldown = 1;
    [SerializeField] private int desintegrationRadius = 5;
    [SerializeField] private int explosionRadius = 10;
    [SerializeField] private int explosionIntensity = 50;
    [SerializeField] private float range;

    private float lastFireTime;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Time.time - lastFireTime > cooldown)
            {
                Fire();
                generator.ApplyChanges();
                lastFireTime = Time.time;
            }
        }
    }

    public void Explode(WorldGenerator.RayWorldInfo info)
    {
        List<Vector3Int> blocks = new List<Vector3Int>();
        Vector3Int center = Vector3Int.FloorToInt(info.pos);

        // Get all blocks within the primary radius
        for (int x = -desintegrationRadius; x <= desintegrationRadius; x++)
        {
            for (int y = -desintegrationRadius; y <= desintegrationRadius; y++)
            {
                for (int z = -desintegrationRadius; z <= desintegrationRadius; z++)
                {
                    Vector3Int blockPos = new Vector3Int(center.x + x, center.y + y, center.z + z);
                    if (Vector3Int.Distance(center, blockPos) <= desintegrationRadius)
                    {
                        generator.RemoveBlock(blockPos);
                    }
                }
            }
        }

        // Add random blocks within the secondary radius
        for (int i = 0; i < explosionIntensity; i++)
        {
            Vector3Int randomBlockPos = center + new Vector3Int(
                Random.Range(-explosionRadius, explosionRadius + 1),
                Random.Range(-explosionRadius, explosionRadius + 1),
                Random.Range(-explosionRadius, explosionRadius + 1));

            if (Vector3Int.Distance(center, randomBlockPos) <= explosionRadius && !blocks.Contains(randomBlockPos))
            {
                generator.RemoveBlock(randomBlockPos);
            }
        }
    }

    public void Fire()
    {
        WorldGenerator.RayWorldInfo fireRes = generator.RayCastWorld(transform.position, transform.forward, range);
        if (fireRes.hit)
        {
            Explode(fireRes);
        }
    }
}
