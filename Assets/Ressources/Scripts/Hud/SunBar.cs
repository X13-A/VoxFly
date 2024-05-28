using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SunBar : MonoBehaviour, IEventHandler
{
    [SerializeField] private Slider slider;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;
    [SerializeField] private float maxBurningRate = 500;

    public void Start()
    {
        slider.maxValue = maxBurningRate;
        slider.value = 0;
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaneStateEvent>(UpdateSlider);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaneStateEvent>(UpdateSlider);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void UpdateSlider(PlaneStateEvent e)
    {
        if (e.eBurningPercent.HasValue)
        {
            slider.value = (e.eBurningPercent.Value / 100) * slider.maxValue;
            gradient.Evaluate(1f);
            fill.color = gradient.Evaluate(slider.normalizedValue);
        }
    }   
}
