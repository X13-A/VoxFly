using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class PlayerManager : MonoBehaviour, IEventHandler
{
    [SerializeField]
    List<GameObject> m_PlayerObjects;

    public void Start()
    {
        SetActiveObjects(false);
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<DisablePlayerEvent>(DisablePlayer);
        EventManager.Instance.AddListener<GamePlayStartEvent>(EnablePlayer);
        EventManager.Instance.AddListener<PausePlayerEvent>(PausePlayer);
        EventManager.Instance.AddListener<ResumePlayerEvent>(ResumePlayer);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<DisablePlayerEvent>(DisablePlayer);
        EventManager.Instance.RemoveListener<GamePlayStartEvent>(EnablePlayer);
        EventManager.Instance.RemoveListener<PausePlayerEvent>(PausePlayer);
        EventManager.Instance.RemoveListener<ResumePlayerEvent>(ResumePlayer);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }
    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void SetActiveObjects(bool b)
    {
        foreach (var obj in m_PlayerObjects)
        {
            obj.SetActive(b);
        }
    }

    void DisablePlayer(DisablePlayerEvent e)
    {
        SetActiveObjects(false);
    }


    void EnablePlayer(GamePlayStartEvent e)
    {
        SetActiveObjects(true);
    }

    void PausePlayer(PausePlayerEvent e)
    {
        SetActiveObjects(false);
    }

    void ResumePlayer(ResumePlayerEvent e)
    {
        SetActiveObjects(true);
    }
}