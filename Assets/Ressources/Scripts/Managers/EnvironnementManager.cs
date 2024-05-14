using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironnementManager : MonoBehaviour, IEventHandler
{
    [SerializeField] private List<int> levels;
    [SerializeField] private Light directionalLight;

    bool flag1 = false;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<UpdateGameScoreEvent>(UpdateEnvironnement);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<UpdateGameScoreEvent>(UpdateEnvironnement);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void UpdateEnvironnement(UpdateGameScoreEvent e)
    {
        if (!flag1 && e.score >= levels[0])
        {
            flag1 = true;
            Color newColor = new Color(171, 171, 171);
            directionalLight.color = newColor;
            EventManager.Instance.Raise(new PlaySoundEvent { eNameClip = "thunder", eLoop = true });
        }
    }
}
