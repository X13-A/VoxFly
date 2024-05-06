using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class PlayerManager : MonoBehaviour, IEventHandler
{
    [SerializeField]
    GameObject m_PlayerObject;

    public void Awake()
    {
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<DisablePlayerEvent>(DisablePlayer);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<DisablePlayerEvent>(DisablePlayer);
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
    }

    void EnablePlayer()
    {
        m_PlayerObject.SetActive(true);
    }

    void Pause()
    {

    }
}