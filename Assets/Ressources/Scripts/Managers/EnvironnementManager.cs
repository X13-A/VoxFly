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

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<UpdateGameScoreEvent>(UpdateEnvironnement);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<UpdateGameScoreEvent>(UpdateEnvironnement);
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

        EventManager.Instance.Raise(new SetGlobalBrightnessEvent() { eValue = 1 });
        EventManager.Instance.Raise(new SetCloudCoverageEvent() { eValue = 0 });
        EventManager.Instance.Raise(new SoundMixEvent() { eGameplayVolume = 0 });
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "wind", eLoop = true });
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "rain", eLoop = true });
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "thunder", eLoop = true });

        levels.Sort();
        levelsReached = new Dictionary<int, bool>();
        foreach (int level in levels)
        {
            levelsReached[level] = false;
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
        float normalizedValue = normalizeLight(level);
        // from 0 to 1 : 0 = black, 1 = full light
        EventManager.Instance.Raise(new SetGlobalBrightnessEvent() { eValue = 1 - normalizedValue });
        // from 0 to 1 : 0 = no clouds, 1 = full clouds
        EventManager.Instance.Raise(new SetCloudCoverageEvent() { eValue = normalizedValue });
        // increase sfx volume
        EventManager.Instance.Raise(new SoundMixEvent() { eGameplayVolume = normalizedValue });
        // increase aircraft disruptions

        // ajouter la pluie
    }

    float normalizeLight(float value)
    {
        float minLevel = levels[0];
        float maxLevel = levels[levels.Count - 1];
        return (value - minLevel) / (maxLevel - minLevel);
    }
}
