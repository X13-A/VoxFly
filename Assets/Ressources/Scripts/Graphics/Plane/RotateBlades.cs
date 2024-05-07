using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBlades : MonoBehaviour
{
    [SerializeField] private Transform blades;
    [SerializeField] private plane planeCommands;
    [SerializeField] private float speedMultiplier = 2000;

    void Update()
    {
        if (planeCommands == null)
        {
            blades.rotation *= Quaternion.Euler(0, 0, speedMultiplier * Time.deltaTime);
        }
        else
        {
            blades.rotation *= Quaternion.Euler(0, 0, (0.1f + planeCommands.Throttle) * speedMultiplier * Time.deltaTime); 
        }
    }
}
