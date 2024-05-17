using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironnementManager : MonoBehaviour, IEventHandler
{
    [SerializeField] private List<int> levels;
    [SerializeField] private GameObject rain;

    private Dictionary<int, bool> levelsReached;
    private ParticleSystem rainPS;
    bool flag = false;

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

        rainPS = rain.GetComponent<ParticleSystem>();
        EventManager.Instance.Raise(new SetGlobalBrightnessEvent() { eValue = 1 });
        EventManager.Instance.Raise(new SetCloudCoverageEvent() { eValue = 0 });
        EventManager.Instance.Raise(new SoundMixEvent() { eGameplayVolume = 0 });
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "wind", eLoop = true });
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "thunder", eLoop = true });
        rain.SetActive(false);
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
        EventManager.Instance.Raise(new SetTurbulenceEvent() { eStrength = normalizedValue });
        // increase rateOverTime of rain
        if (!flag && normalizedValue > 0.6)
        {
            flag = true;
            rain.SetActive(true);
            EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "rain", eLoop = true });
        }
        var emission = rainPS.emission;
        emission.rateOverTime = 100 * normalizedValue;
    }

    float normalizeLight(float value)
    {
        float minLevel = levels[0];
        float maxLevel = levels[levels.Count - 1];
        return (value - minLevel) / (maxLevel - minLevel);
    }
}
