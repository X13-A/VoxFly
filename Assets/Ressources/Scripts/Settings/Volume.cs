using SDD.Events;
using UnityEngine;
using UnityEngine.UI;

public class Volume : MonoBehaviour, IEventHandler
{
    [SerializeField] private Slider menuSlider;
    [SerializeField] private Slider gameplaySlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider planeSlider;
    [SerializeField] private Toggle muteAllToggle;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<SoundMixAllEvent>(SoundMix);
    }

    public void UnsubscribeEvents()
    {
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
        menuSlider.value = AudioManager.Instance.MaxMenuVolume;
        gameplaySlider.value = AudioManager.Instance.MaxGameplayVolume;
        sfxSlider.value = AudioManager.Instance.MaxSFXVolume;
        planeSlider.value = AudioManager.Instance.MaxPlaneVolume;
        muteAllToggle.isOn = AudioManager.Instance.Mute;

        EventManager.Instance.Raise(new SoundMixAllEvent { 
            eSFXVolume = sfxSlider.value,
            eGameplayVolume = gameplaySlider.value,
            eMenuVolume = menuSlider.value,
            ePlaneVolume = planeSlider.value
        });

        menuSlider.onValueChanged.AddListener(MenuSliderChanged);
        gameplaySlider.onValueChanged.AddListener(GameplaySliderChanged);
        sfxSlider.onValueChanged.AddListener(SFXSliderChanged);
        planeSlider.onValueChanged.AddListener(PlaneSliderChanged);
        muteAllToggle.onValueChanged.AddListener(MuteAll);
    }

    void MenuSliderChanged(float value)
    {
        EventManager.Instance.Raise(new SoundMixAllEvent { eMenuVolume = menuSlider.value });
    }

    void GameplaySliderChanged(float value)
    {
        EventManager.Instance.Raise(new SoundMixAllEvent { eGameplayVolume = gameplaySlider.value });
    }

    void SFXSliderChanged(float value)
    {
        EventManager.Instance.Raise(new SoundMixAllEvent { eSFXVolume = sfxSlider.value });
    }

    void PlaneSliderChanged(float value)
    {
        EventManager.Instance.Raise(new SoundMixAllEvent { ePlaneVolume = planeSlider.value });
    }

    private void MuteAll(bool isCheck)
    {
        EventManager.Instance.Raise(new MuteAllSoundEvent { eMute = isCheck });
    }

    void SoundMix(SoundMixAllEvent e)
    {
        if (e.eMenuVolume.HasValue)
        {
            menuSlider.value = e.eMenuVolume.Value;
        }

        if (e.eGameplayVolume.HasValue)
        {
            gameplaySlider.value = e.eGameplayVolume.Value;
        }

        if (e.eSFXVolume.HasValue)
        {
            sfxSlider.value = e.eSFXVolume.Value;
        }

        if (e.ePlaneVolume.HasValue)
        {
            planeSlider.value = e.ePlaneVolume.Value;
        }
    }
}
