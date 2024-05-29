using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironnementManager : MonoBehaviour, IEventHandler
{
    [SerializeField] private List<int> levels;

    private Dictionary<int, bool> levelsReached;
    float maxGameplayVolume = 1;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<UpdateGameScoreEvent>(UpdateEnvironnement);
        EventManager.Instance.AddListener<GamePlayStartEvent>(GamePlayStart);
        EventManager.Instance.AddListener<DestroyEvent>(Destroy);
        EventManager.Instance.AddListener<SoundMixAllEvent>(SoundMix);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<UpdateGameScoreEvent>(UpdateEnvironnement);
        EventManager.Instance.RemoveListener<GamePlayStartEvent>(GamePlayStart);
        EventManager.Instance.RemoveListener<DestroyEvent>(Destroy);
        EventManager.Instance.RemoveListener<SoundMixAllEvent>(SoundMix);
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
        if (levels.Count == 0)
        {
            Debug.LogError("Levels list is empty.");
            return;
        }

        levels.Sort();
        levelsReached = new Dictionary<int, bool>();
        foreach (int level in levels)
        {
            levelsReached[level] = false;
        }
    }

    void SoundMix(SoundMixAllEvent e)
    {
        if (e.eGameplayVolume.HasValue)
        {
            maxGameplayVolume = e.eGameplayVolume.Value;
        }
    }

    void GamePlayStart(GamePlayStartEvent e)
    {
        EventManager.Instance.Raise(new StopSoundEvent() { eNameClip = "explosion" });
        EventManager.Instance.Raise(new SetGlobalBrightnessEvent() { eValue = 1 });
        EventManager.Instance.Raise(new SetCloudCoverageEvent() { eValue = 0 });
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "wind", eLoop = true });
        EventManager.Instance.Raise(new SoundMixSoundEvent() { eNameClip = "wind", eVolume = 0 });
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "thunder", eLoop = true });
        EventManager.Instance.Raise(new SoundMixSoundEvent() { eNameClip = "thunder", eVolume = 0 });
    }

    void Destroy(DestroyEvent e)
    {
        EventManager.Instance.Raise(new StopSoundEvent() { eNameClip = "wind" });
        EventManager.Instance.Raise(new StopSoundEvent() { eNameClip = "thunder" });
    }
    int score = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            score += 100;
            EventManager.Instance.Raise(new UpdateGameScoreEvent() { score = score });
        }
    }

    void UpdateEnvironnement(UpdateGameScoreEvent e)
    {
        foreach (int level in levels)
        {
            if (e.score >= level && !levelsReached[level])
            {
                increaseEnvironnementVariables(level);
                levelsReached[level] = true;
            }
        }
    }

    void increaseEnvironnementVariables(float level)
    {
        float normalizedValue = normalize(level);
        float normalizedSoundValue = normalizedSound(level);
        // from 0 to 1 : 0 = black, 1 = full light
        EventManager.Instance.Raise(new SetGlobalBrightnessEvent() { eValue = 1 - normalizedValue });
        // from 0 to 1 : 0 = no clouds, 1 = full clouds
        EventManager.Instance.Raise(new SetCloudCoverageEvent() { eValue = normalizedValue });
        // increase gameplay volume
        EventManager.Instance.Raise(new SoundMixSoundEvent() { eNameClip = "wind", eVolume = normalizedSoundValue });
        EventManager.Instance.Raise(new SoundMixSoundEvent() { eNameClip = "thunder", eVolume = normalizedSoundValue });
        // increase aircraft disruptions
        EventManager.Instance.Raise(new SetTurbulenceEvent() { eStrength = normalizedValue });
    }

    float normalize(float value)
    {
        float minLevel = levels[0];
        float maxLevel = levels[levels.Count - 1];
        return (value - minLevel) / (maxLevel - minLevel);
    }

    float normalizedSound(float value)
    {
        float minLevel = levels[0];
        float maxLevel = levels[levels.Count - 1];
        float normalizedInToLevel = (value - minLevel) / (maxLevel - minLevel);
        float normalized = normalizedInToLevel * maxGameplayVolume;

        return normalized;
    }
}
