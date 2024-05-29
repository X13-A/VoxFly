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
    [SerializeField] private float transition = 80;

    int burningPercent = 0;
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
    //    if (Input.GetKey(KeyCode.U))
    //    {
    //        burningPercent -= 5;
    //        UpdateFireTest(burningPercent);
    //    }

    //    if (Input.GetKey(KeyCode.O))
    //    {
    //        burningPercent += 5;
    //        UpdateFireTest(burningPercent);
    //    }

    //}

    //void UpdateFireTest(float value)
    //{
    //    Debug.Log("burningPercent : " + value + "/100");

    //    if (value <= 10)
    //    {
    //        smoke.SetActive(false);
    //        fire.SetActive(false);
    //    }
    //    else if (value <= transition)
    //    {
    //        smokeUpdate(value);
    //    }
    //    else
    //    {
    //        fireUpdate(value);
    //    }
    //}

    void UpdateFire(PlaneStateEvent e)
    {
        if (e.eBurningPercent.HasValue)
        {
            if (e.eBurningPercent.Value <= 10)
            {
                smoke.SetActive(false);
                fire.SetActive(false);
            }
            else if (e.eBurningPercent.Value <= transition)
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

        float normalizedPercent = percent / 100.0f;
        float emissionRate = Mathf.Lerp(0, transition, normalizedPercent);

        ParticleSystem smokeParticles = smoke.GetComponent<ParticleSystem>();
        var emission = smokeParticles.emission;
        emission.rateOverTime = emissionRate;
    }

    void fireUpdate(float percent)
    {
        fire.SetActive(true);

        int i = 0;

        foreach (Transform child in fire.transform)
        {
            float normalizedPercent = (percent - transition) / transition;
            float emissionRate = Mathf.Lerp(0, fireEmission[i], normalizedPercent);

            ParticleSystem fireParticles = child.GetComponent<ParticleSystem>();
            var emission = fireParticles.emission;
            emission.rateOverTime = emissionRate;
            i++;
        }
    }
}
