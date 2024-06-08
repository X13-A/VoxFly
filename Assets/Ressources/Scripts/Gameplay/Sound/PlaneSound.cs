using SDD.Events;
using UnityEngine;

public class PlaneSound : MonoBehaviour, IEventHandler
{
    float minThrust = 0;
    float maxThrust = 1;
    float minAudio = 0;
    float maxAudio = 1;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaneStateEvent>(Thrusting);
        EventManager.Instance.AddListener<PlaneInformationEvent>(PlaneInformation);
        EventManager.Instance.AddListener<DestroyEvent>(Destroy);
        EventManager.Instance.AddListener<GamePlayStartEvent>(GamePlayStart);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaneStateEvent>(Thrusting);
        EventManager.Instance.RemoveListener<PlaneInformationEvent>(PlaneInformation);
        EventManager.Instance.RemoveListener<DestroyEvent>(Destroy);
        EventManager.Instance.RemoveListener<GamePlayStartEvent>(GamePlayStart);
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
        maxAudio = AudioManager.Instance.MaxPlaneVolume;
        minAudio = maxAudio * .20f; // 20% of max volume
    }

    void Destroy(DestroyEvent e)
    {
        EventManager.Instance.Raise(new StopSoundEvent() { eNameClip = "plane" });
    }

    void GamePlayStart(GamePlayStartEvent e)
    {
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "plane", eLoop = true });
        EventManager.Instance.Raise(new SoundMixSoundEvent() { eNameClip = "plane", eVolume = minAudio });
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

        if (isInWater)
        {
            EventManager.Instance.Raise(new PlaneMixSoundEvent() { eVolume = volume * .30f, ePitch = .4f });
        }
        else
        {
            EventManager.Instance.Raise(new PlaneMixSoundEvent() { eVolume = volume, ePitch = 1f });

        }
    }
}
