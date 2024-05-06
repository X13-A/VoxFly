using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBlades : MonoBehaviour
{
    [SerializeField] private Transform blades;
    [SerializeField] private Plane planeCommands;
    [SerializeField] private float speedMultiplier = 2000;

    void Update()
    {
        blades.rotation *= Quaternion.Euler(0, 0, (0.1f + planeCommands.Throttle) * speedMultiplier * Time.deltaTime); 
    }
}
