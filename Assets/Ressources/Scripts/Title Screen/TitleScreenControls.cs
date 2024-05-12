using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreenControls : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraRig;
    [SerializeField] private Transform plane;

    [Header("Roll")]
    [SerializeField] private float rollDuration = 5;
    [SerializeField] private AnimationCurve rollCurve;
    [SerializeField] private float rollFactor = 0.2f;
    
    [Header("Roll")]
    [SerializeField] private float pitchDuration = 5;
    [SerializeField] private AnimationCurve pitchCurve;
    [SerializeField] private float pitchFactor = 0.2f;
    
    private Vector3 initialPlaneForward;
    private Vector3 initialPlaneRight;
    private Vector3 initialPlaneUp;

    private void Start()
    {
        initialPlaneForward = plane.forward;
        initialPlaneRight = plane.right;
        initialPlaneUp = plane.up;
    }

    private void Update()
    {
        float tRoll = (Time.time % rollDuration) / rollDuration;
        float roll = rollCurve.Evaluate(tRoll) * 360f * Mathf.Deg2Rad * rollFactor;
        
        float tPitch= (Time.time % pitchDuration) / pitchDuration;
        float pitch = pitchCurve.Evaluate(tPitch) * 360f * Mathf.Deg2Rad * pitchFactor;

        Quaternion rollRotation = Quaternion.AngleAxis(roll * Mathf.Rad2Deg, initialPlaneForward);
        Quaternion pitchRotation = Quaternion.AngleAxis(pitch * Mathf.Rad2Deg, initialPlaneRight);
        plane.forward = initialPlaneForward;
        plane.up = initialPlaneUp;
        plane.right = initialPlaneRight;
        plane.rotation = Quaternion.Euler(plane.rotation.eulerAngles.x, plane.rotation.eulerAngles.y, plane.rotation.eulerAngles.z) * rollRotation * pitchRotation;
    }
}
