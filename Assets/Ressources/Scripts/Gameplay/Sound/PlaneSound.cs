using SDD.Events;
using UnityEngine;

public class PlaneSound : MonoBehaviour, IEventHandler
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] float minAudio = .08f;
    [SerializeField] float maxAudio = .8f;

    float minThrust;
    float maxThrust;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaneStateEvent>(Thrusting);
        EventManager.Instance.AddListener<PlaneInformationEvent>(PlaneInformation);
        EventManager.Instance.AddListener<SoundMixEvent>(SoundMix);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaneStateEvent>(Thrusting);
        EventManager.Instance.RemoveListener<PlaneInformationEvent>(PlaneInformation);
        EventManager.Instance.RemoveListener<SoundMixEvent>(SoundMix);
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

    void SoundMix(SoundMixEvent e)
    {
        maxAudio = e.eGameplayVolume;
    }

    void PlaneInformation(PlaneInformationEvent e)
    {
        minThrust = e.eMinThrust;
        maxThrust = e.eMaxThrust;
    }

    void Thrusting(PlaneStateEvent e)
    {
        if (e.eThrust != 0) UpdateVolume(e.eThrust);
    }

    public void UpdateVolume(float currentThrust)
    {
        float normalizedThrust = (currentThrust - minThrust) / (maxThrust - minThrust);
        float volume = normalizedThrust * (maxAudio - minAudio) + minAudio;

        volume = Mathf.Clamp(volume, minAudio, maxAudio);
        audioSource.volume = volume;
    }
}
