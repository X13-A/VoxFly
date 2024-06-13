using SDD.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnvironnementManager : MonoBehaviour, IEventHandler
{
    [SerializeField] private AnimationCurve densityCurve;
    [SerializeField] private AnimationCurve coverageCurve;
    [SerializeField] private AnimationCurve turbulenceCurve;
    [SerializeField] private AnimationCurve soundCurve;
    [SerializeField] private float turbulenceScale = 2;


    [SerializeField] private float scoreStart = 5000;
    [SerializeField] private float scoreEnd = 10000;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<UpdateGameScoreEvent>(UpdateEnvironnement);
        EventManager.Instance.AddListener<GamePlayStartEvent>(GamePlayStart);
        EventManager.Instance.AddListener<DestroyEvent>(Destroy);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<UpdateGameScoreEvent>(UpdateEnvironnement);
        EventManager.Instance.RemoveListener<GamePlayStartEvent>(GamePlayStart);
        EventManager.Instance.RemoveListener<DestroyEvent>(Destroy);
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void GamePlayStart(GamePlayStartEvent e)
    {
        EventManager.Instance.Raise(new StopSoundEvent() { eNameClip = "explosion" });
        EventManager.Instance.Raise(new SetCloudDensityEvent() { eValue = 1 });
        EventManager.Instance.Raise(new SetCloudCoverageEvent() { eValue = 0.2f });
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "wind", eLoop = true });
        EventManager.Instance.Raise(new SoundMixSoundEvent() { eNameClip = "wind", eVolume = 0 });
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "thunder", eLoop = true });
        EventManager.Instance.Raise(new SoundMixSoundEvent() { eNameClip = "thunder", eVolume = 0 });
    }

    private void Destroy(DestroyEvent e)
    {
        EventManager.Instance.Raise(new StopSoundEvent() { eNameClip = "wind" });
        EventManager.Instance.Raise(new StopSoundEvent() { eNameClip = "thunder" });
    }

    private void UpdateEnvironnement(UpdateGameScoreEvent e)
    {
        UpdateEnvironmentVariables(e.score);
    }

    private void UpdateEnvironmentVariables(float score)
    {
        float normalizedScore = NormalizeScore(score);
        float densityValue = EvaluateDensity(normalizedScore);
        float coverageValue = EvaluateCoverage(normalizedScore);
        float turbulenceValue = EvaluateTurbulence(normalizedScore);
        float soundValue = EvaluateSound(normalizedScore);

        // from 0 to 1 : 0 = low density, 1 = high
        EventManager.Instance.Raise(new SetCloudDensityEvent() { eValue = densityValue });
        // from 0 to 1 : 0 = no clouds, 1 = full clouds
        EventManager.Instance.Raise(new SetCloudCoverageEvent() { eValue = coverageValue });
        // increase gameplay volume
        EventManager.Instance.Raise(new SoundMixSoundEvent() { eNameClip = "wind", eVolume = soundValue });
        EventManager.Instance.Raise(new SoundMixSoundEvent() { eNameClip = "thunder", eVolume = soundValue });
        // increase aircraft disruptions
        EventManager.Instance.Raise(new SetTurbulenceEvent() { eStrength = turbulenceValue, eScale = turbulenceScale });
    }

    private float EvaluateDensity(float score)
    {
        return densityCurve.Evaluate(score);
    }

    private float EvaluateCoverage(float score)
    {
        return coverageCurve.Evaluate(score);
    }
    private float EvaluateTurbulence(float score)
    {
        return turbulenceCurve.Evaluate(score);
    }

    private float EvaluateSound(float score)
    {
        return soundCurve.Evaluate(score);
    }

    private float NormalizeScore(float score)
    {
        return Mathf.Clamp01((score - scoreStart) / (scoreEnd - scoreStart));
    }
}
