using SDD.Events;
using UnityEngine;

public class PlaneSound : MonoBehaviour, IEventHandler
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] float minAudio = .08f;

    AudioManager audioManager;
    private float maxAudio => audioManager.maxPlaneVolume;

    float minThrust;
    float maxThrust;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaneStateEvent>(Thrusting);
        EventManager.Instance.AddListener<PlaneInformationEvent>(PlaneInformation);
        EventManager.Instance.AddListener<GamePlayEvent>(SwitchOn);
        EventManager.Instance.AddListener<GameOverEvent>(SwitchOff);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaneStateEvent>(Thrusting);
        EventManager.Instance.RemoveListener<PlaneInformationEvent>(PlaneInformation);
        EventManager.Instance.RemoveListener<GamePlayEvent>(SwitchOn);
        EventManager.Instance.RemoveListener<GameOverEvent>(SwitchOff);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void SwitchOn(GamePlayEvent e)
    {
        audioSource.Play();
    }

    void SwitchOff(GameOverEvent e)
    {
        audioSource.Stop();
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
