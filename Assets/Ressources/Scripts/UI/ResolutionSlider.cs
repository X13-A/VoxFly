using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class ResolutionSlider : MonoBehaviour, IEventHandler
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;
    private string resolutionString => $"({ScreenManager.Instance?.GBufferWidth} x {ScreenManager.Instance?.GBufferHeight})";
    private void OnEnable()
    {
        SubscribeEvents();
        if (ScreenManager.Instance != null)
        {
            slider.value = ScreenManager.Instance.GBufferResolutionScale;
        }
        slider.onValueChanged.AddListener(HandleSliderChange);
        text.text = resolutionString;
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        slider.onValueChanged.RemoveListener(HandleSliderChange);
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<ScreenResolutionChangedEvent>(HandleScreenResolutionChangeEvent);
        EventManager.Instance.AddListener<PausePlayerEvent>(HandlePause);
        EventManager.Instance.AddListener<GameSettingsEvent>(HandleSettings);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<ScreenResolutionChangedEvent>(HandleScreenResolutionChangeEvent);
        EventManager.Instance.RemoveListener<PausePlayerEvent>(HandlePause);
        EventManager.Instance.RemoveListener<GameSettingsEvent>(HandleSettings);
    }
    private void HandlePause(PausePlayerEvent e)
    {
        text.text = resolutionString;
    }

    /// <summary>
    /// TODO: fix
    /// </summary>
    private void HandleSettings(GameSettingsEvent e)
    {
        text.text = resolutionString;
    }

    private void HandleScreenResolutionChangeEvent(ScreenResolutionChangedEvent e)
    {
        text.text = resolutionString;
    }

    private void HandleSliderChange(float t)
    {
        EventManager.Instance.Raise(new GBufferScaleSliderEvent { value = t });
    }
}
