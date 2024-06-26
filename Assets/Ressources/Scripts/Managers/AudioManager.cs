using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>, IEventHandler
{
    [Header("UI")]
    [SerializeField] private float maxSFXVolume = 1;
    [SerializeField] private AudioClip click;

    [Header("Gameplay")]
    [SerializeField] private float maxGameplayVolume = 1;
    [SerializeField] private AudioClip wind;
    [SerializeField] private AudioClip thunder;
    [SerializeField] private AudioClip rain;
    [SerializeField] private AudioClip explosion;
    [SerializeField] private AudioClip score1;
    [SerializeField] private AudioClip score2;

    [Header("Music")]
    [SerializeField] private float maxMenuVolume = 1;
    [SerializeField] private AudioClip menu;

    [Header("Plane")]
    [SerializeField] private float maxPlaneVolume = 1;
    [SerializeField] private AudioClip plane;

    Dictionary<string, Tuple<AudioClip, string>> audioClips;
    List<Tuple<string, float>> audioTypes;
    float saveSFXVolume = 0;
    float saveGameplayVolume = 0;
    float saveMenuVolume = 0;
    float savePlaneVolume = 0;
    bool mute = false;
    public float MaxSFXVolume => maxSFXVolume;
    public float MaxGameplayVolume => maxGameplayVolume;
    public float MaxMenuVolume => maxMenuVolume;
    public float MaxPlaneVolume => maxPlaneVolume;
    public bool Mute => mute;

    protected override void Awake()
    {
        base.Awake();
        /* /!\ Add all audio clips to the dictionary /!\ */
        audioClips = new Dictionary<string, Tuple<AudioClip, string>>()
        {
            { "click", Tuple.Create(click, "sfx") },
            { "wind", Tuple.Create(wind, "gameplay") },
            { "thunder", Tuple.Create(thunder, "gameplay") },
            { "rain", Tuple.Create(rain, "gameplay") },
            { "explosion", Tuple.Create(explosion, "gameplay") },
            { "score1", Tuple.Create(score1, "gameplay") },
            { "score2", Tuple.Create(score2, "gameplay") },
            { "plane", Tuple.Create(plane, "plane") },
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
            Tuple.Create("menu", maxMenuVolume),
            Tuple.Create("plane", maxPlaneVolume)
        };
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaySoundEvent>(PlaySound);
        EventManager.Instance.AddListener<StopSoundEvent>(StopSound);
        EventManager.Instance.AddListener<StopSoundAllEvent>(StopAllSound);
        EventManager.Instance.AddListener<SoundMixAllEvent>(MixSFX);
        EventManager.Instance.AddListener<StopSoundByTypeEvent>(StopSoundByType);
        EventManager.Instance.AddListener<MuteAllSoundEvent>(MuteAllSound);
        EventManager.Instance.AddListener<SoundMixSoundEvent>(SoundMixSound);
        EventManager.Instance.AddListener<PlaneMixSoundEvent>(PlaneMixSound);
        EventManager.Instance.AddListener<PauseAllSoundEvent>(PauseAllSound);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaySoundEvent>(PlaySound);
        EventManager.Instance.RemoveListener<StopSoundEvent>(StopSound);
        EventManager.Instance.RemoveListener<StopSoundAllEvent>(StopAllSound);
        EventManager.Instance.RemoveListener<SoundMixAllEvent>(MixSFX);
        EventManager.Instance.RemoveListener<StopSoundByTypeEvent>(StopSoundByType);
        EventManager.Instance.RemoveListener<MuteAllSoundEvent>(MuteAllSound);
        EventManager.Instance.RemoveListener<SoundMixSoundEvent>(SoundMixSound);
        EventManager.Instance.RemoveListener<PlaneMixSoundEvent>(PlaneMixSound);
        EventManager.Instance.RemoveListener<PauseAllSoundEvent>(PauseAllSound);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void SoundMixSound(SoundMixSoundEvent e)
    {
        GameObject childObject = GameObject.Find(e.eNameClip);
        if (childObject == null) return;
        float max = getMaxVolumeByName(e.eNameClip);
        float volume = max * e.eVolume;
        AudioSource audioSource = childObject.GetComponent<AudioSource>();
        audioSource.volume = volume;
    }

    void MuteAllSound(MuteAllSoundEvent e)
    {
        if (e.eMute)
        {
            mute = true;
            saveGameplayVolume = maxGameplayVolume;
            saveMenuVolume = maxMenuVolume;
            saveSFXVolume = maxSFXVolume;
            savePlaneVolume = maxPlaneVolume;
            EventManager.Instance.Raise(new SoundMixAllEvent()
            {
                eGameplayVolume = 0,
                eMenuVolume = 0,
                eSFXVolume = 0,
                ePlaneVolume = 0
            });
        }
        else
        {
            mute = false;
            EventManager.Instance.Raise(new SoundMixAllEvent()
            {
                eGameplayVolume = saveGameplayVolume,
                eMenuVolume = saveMenuVolume,
                eSFXVolume = saveSFXVolume,
                ePlaneVolume = savePlaneVolume
            });
        }
    }

    void PauseAllSound(PauseAllSoundEvent e)
    {
        if (e.ePause)
        {
            foreach (Transform child in transform)
            {
                AudioSource audioSource = child.GetComponent<AudioSource>();
                audioSource.Pause();
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                AudioSource audioSource = child.GetComponent<AudioSource>();
                audioSource.UnPause();
            }
        }
    }

    void MixSFX(SoundMixAllEvent e)
    {
        if (e.eSFXVolume.HasValue)
        {
            maxSFXVolume = e.eSFXVolume.Value;
        }

        if (e.eGameplayVolume.HasValue)
        {
            maxGameplayVolume = e.eGameplayVolume.Value;
        }

        if (e.eMenuVolume.HasValue)
        {
            maxMenuVolume = e.eMenuVolume.Value;
        }

        if (e.ePlaneVolume.HasValue)
        {
            maxPlaneVolume = e.ePlaneVolume.Value;
        }

        UpdateVolumeList();
        UpdateSound();
    }

    void PlaneMixSound(PlaneMixSoundEvent e)
    {
        GameObject childObject = GameObject.Find("plane");
        if (childObject == null) return;
        AudioSource audioSource = childObject.GetComponent<AudioSource>();

        if (e.eVolume.HasValue)
        {
            audioSource.volume = e.eVolume.Value;
        }

        if (e.ePitch.HasValue)
        {
            audioSource.pitch = e.ePitch.Value;
        }
    }

    void PlaySound(PlaySoundEvent e)
    {
        AudioSource audioSource;
        Transform soundTransform = GetExistingClip(e.eNameClip);
        if (soundTransform != null && e.eCanStack == false)
        {
            return;
        }

        GameObject childObject = new GameObject(e.eNameClip);
        childObject.transform.parent = transform;
        audioSource = childObject.AddComponent<AudioSource>();
        audioClips.TryGetValue(e.eNameClip, out Tuple<AudioClip, string> audioClip);
        audioSource.time = 0;
        audioSource.clip = audioClip.Item1;
        audioSource.volume = getVolume(audioClip.Item2) * e.eVolumeMultiplier;
        audioSource.loop = e.eLoop;
        audioSource.pitch = e.ePitch;
        audioSource.Play();

        if (e.eDestroyWhenFinished)
        {
            StartCoroutine(DestroyAfterPlaying(audioSource));
        }
    }

    /// <summary>
    /// Wait until the audio clip has finished playing, then destroy it
    /// </summary>
    private IEnumerator DestroyAfterPlaying(AudioSource audioSource)
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        Destroy(audioSource.gameObject);
    }

    Transform GetExistingClip(string name)
    {
        foreach (Transform child in transform)
        {
            if (child.name == name)
            {
                return child;
            }
        }
        return null;
    }

    void StopSound(StopSoundEvent e)
    {
        GameObject childObject = GameObject.Find(e.eNameClip);
        if (childObject == null) return;
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

    float getMaxVolumeByName(string name)
    {
        audioClips.TryGetValue(name, out Tuple<AudioClip, string> audioClip);
        return getVolume(audioClip.Item2);
    }
}
