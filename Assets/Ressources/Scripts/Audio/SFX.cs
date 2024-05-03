using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFX : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("SFX")]
    [SerializeField] private AudioClip click;

    AudioManager audioManager;
    private float maxVolume => audioManager.maxSFXVolume;

    private void Start()
    {
        
    }
}
