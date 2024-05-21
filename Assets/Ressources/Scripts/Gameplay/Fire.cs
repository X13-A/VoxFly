using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Fire : MonoBehaviour, IEventHandler
{
    [SerializeField] private GameObject smoke;
    [SerializeField] private GameObject fire;

    //int burningPercent = 0;
    List<float> fireEmission = new List<float>();

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlaneStateEvent>(UpdateFire);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlaneStateEvent>(UpdateFire);
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
        foreach (Transform child in fire.transform)
        {
            ParticleSystem fireParticles = child.GetComponent<ParticleSystem>();
            {
                var emission_f = fireParticles.emission;
                fireEmission.Add(emission_f.rateOverTime.constant);
                emission_f.rateOverTime = 0;
            }
        }

        ParticleSystem smokeParticles = smoke.GetComponent<ParticleSystem>();
        var emission_s = smokeParticles.emission;
        emission_s.rateOverTime = 0;

        smoke.SetActive(false);
        fire.SetActive(false);
    }

    /* For testing */
    //void Update()
    //{
    //    if (Input.GetKey(KeyCode.RightControl))
    //    {
    //        burningPercent += 5;
    //        UpdateFire();
    //    }

    //    if (Input.GetKey(KeyCode.LeftControl))
    //    {
    //        burningPercent -= 5;
    //        UpdateFire();
    //    }
    //}

    void UpdateFire(PlaneStateEvent e)
    {
        if (e.eBurningPercent.HasValue)
        {
            if (e.eBurningPercent.Value <= 50)
            {
                smokeUpdate(e.eBurningPercent.Value);
            }
            else
            {
                fireUpdate(e.eBurningPercent.Value);
            }
        }        
    }

    void smokeUpdate(float percent)
    {
        smoke.SetActive(true);
        fire.SetActive(false);

        float normalizedPercent = Mathf.Clamp(percent / 100.0f, 0.0f, 1.0f);
        float emissionRate = Mathf.Lerp(0, 50, normalizedPercent);

        ParticleSystem smokeParticles = smoke.GetComponent<ParticleSystem>();
        var emission = smokeParticles.emission;
        emission.rateOverTime = emissionRate;
    }

    void fireUpdate(float percent)
    {
        smoke.SetActive(false);
        fire.SetActive(true);

        int i = 0;

        foreach (Transform child in fire.transform)
        {
            float normalizedPercent = Mathf.Clamp((percent - 50) / 50.0f, 0.0f, 1.0f);
            float emissionRate = Mathf.Lerp(0, fireEmission[i], normalizedPercent);

            ParticleSystem fireParticles = child.GetComponent<ParticleSystem>();
            var emission = fireParticles.emission;
            emission.rateOverTime = emissionRate;
            i++;
        }
    }
}
