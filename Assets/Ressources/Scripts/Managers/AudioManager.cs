using SDD.Events;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour, IEventHandler
{
    public float maxSFXVolume = 1;
    public float maxBackgroundVolume = 1;
    public float maxPlaneVolume = 1;

    List<AudioSource> childsAudioSource = new List<AudioSource>();

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<SoundMixEvent>(SoundMix);
        EventManager.Instance.AddListener<SoundMuteEvent>(SoundMute);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<SoundMixEvent>(SoundMix);
        EventManager.Instance.RemoveListener<SoundMuteEvent>(SoundMute);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }
    void Awake()
    {
        foreach (Transform child in transform)
        {
            childsAudioSource.Add(child.GetComponent<AudioSource>());
        }
    }

    void SoundMix(SoundMixEvent e)
    {
        maxSFXVolume = e.eSFXVolume;
        maxBackgroundVolume = e.eBackgroundVolume;
        maxPlaneVolume = e.ePlaneVolume;
    }

    void SoundMute(SoundMuteEvent e)
    {
        if (e.eMute)
        {
            foreach (AudioSource audioSource in childsAudioSource)
            {
                audioSource.Play();
            }
        } 
        else
        {
            foreach (AudioSource audioSource in childsAudioSource)
            {
                audioSource.Stop();
            }
        }
    }
}
