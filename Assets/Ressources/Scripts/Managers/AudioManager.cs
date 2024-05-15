using SDD.Events;
using System;
using System.Collections;
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

    Dictionary<string, Tuple<AudioClip, string>> audioClips;
    List<Tuple<string, float>> audioTypes;

    void Awake()
    {
        /* /!\ Add all audio clips to the dictionary /!\ */
        audioClips = new Dictionary<string, Tuple<AudioClip, string>>()
        {
            { "click", Tuple.Create(click, "sfx") },
            { "wind", Tuple.Create(wind, "gameplay") },
            { "thunder", Tuple.Create(thunder, "gameplay") },
            { "rain", Tuple.Create(rain, "gameplay") },
            { "menu", Tuple.Create(menu, "menu") }
        };

        /* /!\ Add all audio types to the lists in UpdateVolumeList() function /!\ */
        UpdateVolumeList();
    }

    void UpdateVolumeList()
    {
        audioTypes = new List<Tuple<string, float>>()
        {
            Tuple.Create("sfx", maxSFXVolume),
            Tuple.Create("gameplay", maxGameplayVolume),
            Tuple.Create("menu", maxMenuVolume)
        };
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaySoundEvent>(PlaySound);
        EventManager.Instance.AddListener<StopSoundEvent>(StopSound);
        EventManager.Instance.AddListener<StopSoundAllEvent>(StopAllSound);
        EventManager.Instance.AddListener<SoundMixEvent>(MixSFX);
        EventManager.Instance.AddListener<StopSoundByTypeEvent>(StopSoundByType);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaySoundEvent>(PlaySound);
        EventManager.Instance.RemoveListener<StopSoundEvent>(StopSound);
        EventManager.Instance.RemoveListener<StopSoundAllEvent>(StopAllSound);
        EventManager.Instance.RemoveListener<SoundMixEvent>(MixSFX);
        EventManager.Instance.RemoveListener<StopSoundByTypeEvent>(StopSoundByType);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void MixSFX(SoundMixEvent e)
    {
        if (e.eSFXVolume.HasValue)
        {
            maxSFXVolume = e.eSFXVolume.Value;
            Debug.Log("SFX Volume: " + maxSFXVolume);
        }

        if (e.eGameplayVolume.HasValue)
        {
            maxGameplayVolume = e.eGameplayVolume.Value;
            Debug.Log("Gameplay Volume: " + maxGameplayVolume);
        }

        if (e.eMenuVolume.HasValue)
        {
            maxMenuVolume = e.eMenuVolume.Value;
            Debug.Log("Menu Volume: " + maxMenuVolume);
        }

        UpdateVolumeList();
        UpdateSound();
    }

    void PlaySound(PlaySoundEvent e)
    {
        GameObject childObject = new GameObject(e.eNameClip);
        childObject.transform.parent = transform;
        AudioSource audioSource = childObject.AddComponent<AudioSource>();
        audioClips.TryGetValue(e.eNameClip, out Tuple<AudioClip, string> audioClip);
        audioSource.clip = audioClip.Item1;
        audioSource.volume = getVolume(audioClip.Item2);
        audioSource.loop = e.eLoop;
        audioSource.Play();
    }

    void StopSound(StopSoundEvent e)
    {
        GameObject childObject = GameObject.Find(e.eNameClip);
        Destroy(childObject);
    }

    void StopSoundByType(StopSoundByTypeEvent e)
    {
        foreach (Transform child in transform)
        {
            audioClips.TryGetValue(child.name, out Tuple<AudioClip, string> audioClip);
            if (audioClip.Item2 == e.eType)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void StopAllSound(StopSoundAllEvent e)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    void UpdateSound()
    {
        foreach (Transform child in transform)
        {
             AudioSource audioSource = child.GetComponent<AudioSource>();
            audioClips.TryGetValue(child.name, out Tuple<AudioClip, string> audioClip);
            audioSource.volume = getVolume(audioClip.Item2);
        }
    }

    float getVolume(string type)
    {
        foreach (Tuple<string, float> audioType in audioTypes)
        {
            if (audioType.Item1 == type)
            {
                return audioType.Item2;
            }
        }

        return 0;
    }   
}
