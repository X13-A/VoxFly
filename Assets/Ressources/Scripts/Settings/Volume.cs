using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Volume : MonoBehaviour
{
    [SerializeField] private Slider menuSlider;
    [SerializeField] private Slider gameplaySlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle muteAllToggle;

    float menuVolume;
    float gameplayVolume;
    float sfxVolume;

    void Start()
    {
        menuSlider.value = .8f;
        gameplaySlider.value = .8f;
        sfxSlider.value = .8f;
        muteAllToggle.isOn = false;

        menuVolume = menuSlider.value;
        gameplayVolume = gameplaySlider.value;
        sfxVolume = sfxSlider.value;

        EventManager.Instance.Raise(new SoundMixEvent { 
            eSFXVolume = sfxSlider.value,
            eGameplayVolume = gameplaySlider.value,
            eMenuVolume = menuSlider.value
        });

        EventManager.Instance.Raise(new PlaySoundEvent()
        {
            eNameClip = "menu",
            eLoop = true
        });

        menuSlider.onValueChanged.AddListener(SliderChanged);
        gameplaySlider.onValueChanged.AddListener(SliderChanged);
        sfxSlider.onValueChanged.AddListener(SliderChanged);
        muteAllToggle.onValueChanged.AddListener(MuteAll);
    }

    private void SliderChanged(float value)
    {
        EventManager.Instance.Raise(new SoundMixEvent
        {
            eSFXVolume = sfxSlider.value,
            eGameplayVolume = gameplaySlider.value,
            eMenuVolume = menuSlider.value
        });

        menuVolume = menuSlider.value;
        gameplayVolume = gameplaySlider.value;
        sfxVolume = sfxSlider.value;
    }

    private void MuteAll(bool isCheck)
    {
        Debug.Log("MuteAll: " + isCheck);
        if (isCheck)
        {
            EventManager.Instance.Raise(new SoundMixEvent
            {
                eSFXVolume = 0,
                eGameplayVolume = 0,
                eMenuVolume = 0
            });
        }
        else
        {
            EventManager.Instance.Raise(new SoundMixEvent
            {
                eSFXVolume = menuVolume,
                eGameplayVolume = gameplayVolume,
                eMenuVolume = sfxVolume
            });

            menuSlider.value = menuVolume;
            gameplaySlider.value = gameplayVolume;
            sfxSlider.value = sfxVolume;
        }
    }
}
