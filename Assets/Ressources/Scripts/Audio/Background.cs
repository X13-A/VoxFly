using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Menu")]
    [SerializeField] private AudioClip menu;

    [Header("Gameplay")]
    [SerializeField] private AudioClip wind;
    [SerializeField] private AudioClip thunder;
    [SerializeField] private AudioClip rain;

    AudioManager audioManager;
    private float maxVolume => audioManager.maxBackgroundVolume;

    private void Start()
    {
        
    }
}
