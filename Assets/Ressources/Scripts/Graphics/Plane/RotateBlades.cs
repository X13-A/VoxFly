using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBlades : MonoBehaviour
{
    [SerializeField] private Transform blades;
    [SerializeField] private float speed = 1;

    void Update()
    {
        blades.rotation *= Quaternion.Euler(0, 0, speed * Time.deltaTime); 
    }
}
