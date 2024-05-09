using SDD.Events;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour, IEventHandler
{
    [Header("SFX")]
    [SerializeField] private float maxSFXVolume = 1;
    [SerializeField] private AudioClip click;

    [Header("Gameplay")]
    [SerializeField] private float maxGameplayVolume = 1;
    [SerializeField] private AudioClip wind;
    [SerializeField] private AudioClip thunder;
    [SerializeField] private AudioClip rain;

    [Header("Menu")]
    [SerializeField] private float maxMenuVolume = 1;
    [SerializeField] private AudioClip menu;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaySoundEvent>(PlaySound);
        EventManager.Instance.AddListener<StopSoundEvent>(StopSound);
        EventManager.Instance.AddListener<StopSoundAllEvent>(StopAllSound);
        EventManager.Instance.AddListener<SoundMixEvent>(MixSFX);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaySoundEvent>(PlaySound);
        EventManager.Instance.RemoveListener<StopSoundEvent>(StopSound);
        EventManager.Instance.RemoveListener<StopSoundAllEvent>(StopAllSound);
        EventManager.Instance.RemoveListener<SoundMixEvent>(MixSFX);
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
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "rain", eLoop = true });
    }

    void MixSFX(SoundMixEvent e)
    {
        maxSFXVolume = e.eSFXVolume;
        maxGameplayVolume = e.eGameplayVolume;
        maxMenuVolume = e.eMenuVolume;
        UpdateSound();
    }

    void PlaySound(PlaySoundEvent e)
    {
        GameObject childObject = new GameObject(e.eNameClip);
        childObject.transform.parent = transform;
        AudioSource audioSource = childObject.AddComponent<AudioSource>();
        Tuple<AudioClip, float> audioClip = findAudioClip(e.eNameClip);
        audioSource.clip = audioClip.Item1;
        audioSource.volume = audioClip.Item2;
        audioSource.loop = e.eLoop;
        audioSource.Play();
    }

    void StopSound(StopSoundEvent e)
    {
        GameObject childObject = GameObject.Find(e.eNameClip);
        Destroy(childObject);
    }

    void StopAllSound(StopSoundAllEvent e)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    Tuple<AudioClip, float> findAudioClip(string clip)
    {
        AudioClip audioClip;
        float volume;

        switch (clip)
        {
            case "click":
                audioClip = click;
                volume = maxSFXVolume;
                break;
            case "wind":
                audioClip = wind;
                volume = maxGameplayVolume;
                break;
            case "thunder":
                audioClip = thunder;
                volume = maxGameplayVolume;
                break;
            case "rain":
                audioClip = rain;
                volume = maxGameplayVolume;
                break;
            case "menu":
                audioClip = menu;
                volume = maxMenuVolume;
                break;
            default:
                audioClip = null;
                volume = 0;
                break;
        }

        return new Tuple<AudioClip, float>(audioClip, volume);
    }

    void UpdateSound()
    {
        foreach (Transform child in transform)
        {
            AudioSource audioSource = child.GetComponent<AudioSource>();
            Tuple<AudioClip, float> audioClip = findAudioClip(child.name);
            audioSource.volume = audioClip.Item2;
        }
    }
}
