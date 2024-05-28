using SDD.Events;
using UnityEngine;

public class PlaneSound : MonoBehaviour, IEventHandler
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] float minAudio;
    [SerializeField] float maxAudio;

    float minThrust;
    float maxThrust;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaneStateEvent>(Thrusting);
        EventManager.Instance.AddListener<PlaneInformationEvent>(PlaneInformation);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaneStateEvent>(Thrusting);
        EventManager.Instance.RemoveListener<PlaneInformationEvent>(PlaneInformation);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void Start()
    {
        audioSource.volume = minAudio;
    }

    void PlaneInformation(PlaneInformationEvent e)
    {
        minThrust = e.eMinThrust;
        maxThrust = e.eMaxThrust;
    }

    void Thrusting(PlaneStateEvent e)
    {
        if (e.eThrust.HasValue && e.eIsInWater.HasValue)
        {
            if (e.eThrust != 0) UpdateVolume(e.eThrust.Value, e.eIsInWater.Value);
        }
    }

    public void UpdateVolume(float currentThrust, bool isInWater = false)
    {
        float normalizedThrust = (currentThrust - minThrust) / (maxThrust - minThrust);
        float volume = normalizedThrust * (maxAudio - minAudio) + minAudio;

        volume = Mathf.Clamp(volume, minAudio, maxAudio);
        audioSource.volume = volume;

        if (isInWater)
        {
            audioSource.volume = Mathf.Max(audioSource.volume, 0.3f);
            audioSource.pitch = 0.4f;
        }
        else
        {
            audioSource.pitch = 1f;
        }
    }
}
