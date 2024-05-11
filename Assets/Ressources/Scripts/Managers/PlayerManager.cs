using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class PlayerManager : MonoBehaviour, IEventHandler
{
    [SerializeField]
    GameObject m_PlayerObject;
    [SerializeField]
    GameObject m_HudObject;

    public void Start()
    {
        m_PlayerObject.SetActive(false);
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

    void DisablePlayer(DisablePlayerEvent e)
    {
        m_PlayerObject.SetActive(false);
        m_HudObject.SetActive(false);
    }

    void InitPlayer(WorldGeneratedEvent e)
    {
        m_PlayerObject.SetActive(false);
    }

    void EnablePlayer(GamePlayStartEvent e)
    {
        m_PlayerObject.SetActive(true);
        m_HudObject.SetActive(true);
    }

    void PausePlayer(PausePlayerEvent e)
    {
        m_PlayerObject.SetActive(false);
        m_HudObject.SetActive(false);
    }

    void ResumePlayer(ResumePlayerEvent e)
    {
        m_PlayerObject.SetActive(true);
        m_HudObject.SetActive(true);
    }
}