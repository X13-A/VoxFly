using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private Light sunLight; // Reference to the directional light
    [SerializeField] private float dayDuration = 60f; // Duration of a full day in seconds
    [SerializeField] private float timeScale = 1f; // Speed of the day-night cycle

    private float currentTime = 0f; // Current time in seconds

    void Update()
    {
        // Update the current time based on the time scale
        currentTime += Time.deltaTime * timeScale;

        // Calculate the angle of rotation for the sun based on the current time
        float angle = (currentTime / dayDuration) * 360f;

        // Apply the rotation to the directional light
        sunLight.transform.rotation = Quaternion.Euler(angle, 0f, 0f);

        // Clamp the current time to ensure it stays within one day
        currentTime %= dayDuration;
    }
}
