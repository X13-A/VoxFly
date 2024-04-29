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
    [SerializeField] private float maxBurningRate;

    public void Start()
    {
        slider.maxValue = maxBurningRate;
        slider.value = 0;
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaneIsInShadowEvent>(IsInShadow);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaneIsInShadowEvent>(IsInShadow);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void IsInShadow(PlaneIsInShadowEvent e)
    {
        if (e.eIsInShadow)
        {
            slider.value -= e.eRayRate;
        }
        else
        {
            slider.value += e.eRayRate;
        }

        gradient.Evaluate(1f);
        fill.color = gradient.Evaluate(slider.normalizedValue);

        if (slider.value >= maxBurningRate)
        {
            EventManager.Instance.Raise(new GameOverEvent());
        }
        else
        {
            EventManager.Instance.Raise(new PlaneStateEvent() { eBurningRate = slider.value * 100 / slider.maxValue });
        }
    }   
}
