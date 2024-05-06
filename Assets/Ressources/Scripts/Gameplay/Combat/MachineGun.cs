using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MachineGun : MonoBehaviour
{
    [SerializeField] private WorldGenerator generator;
    [SerializeField] private float fireRate;
    [SerializeField] private float fireSpread;
    [SerializeField] private float range;
    [SerializeField] private float updateDelay;

    private int changes;
    private float lastFireTime;
    private float lastUpdateTime;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Time.time - lastFireTime > 1f / fireRate)
            {
                for (int i = 0; i < 50; i++)
                {
                    Fire();
                }
                lastFireTime = Time.time;
            }
        }
        if (Time.time - lastUpdateTime > updateDelay)
        {
            if (changes > 0)
            {
                generator.ApplyChanges();
                changes = 0;
            }
            lastUpdateTime = Time.time;
        }
    }

    public void Fire()
    {
        Vector3 fireDir = transform.forward;
        fireDir += transform.right * Random.Range(-fireSpread/2, fireSpread/2);
        fireDir += transform.up * Random.Range(-fireSpread/2, fireSpread/2);
        fireDir.Normalize();

        WorldGenerator.RayWorldInfo fireRes = generator.RayCastWorld(transform.position, fireDir, range);
        if (fireRes.hit)
        {
                
            generator.RemoveBlock(fireRes.pos + fireDir * 0.1f);
            changes++;
            //Debug.Log($"Hit ! Depth: {fireRes.depth} Block: {fireRes.BlockID}");
        }
        else
        {
            //Debug.Log("Miss !");
        }
    }
}
