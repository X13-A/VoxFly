using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class PauseManager : MonoBehaviour, IEventHandler
{

    private bool m_CanClick = true;
    [SerializeField] GameObject m_PausePanel;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GamePauseEvent>(Pause);
        EventManager.Instance.AddListener<GameResumeEvent>(Resume);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GamePauseEvent>(Pause);
        EventManager.Instance.RemoveListener<GameResumeEvent>(Resume);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }
    void OnDisable()
    {
        UnsubscribeEvents();
    }

    public void PauseButtonnClicked()
    {
        EventManager.Instance.Raise(new PauseButtonClickedEvent());
    }

    public void Pause(GamePauseEvent e)
    {
        m_PausePanel.SetActive(true);
        EventManager.Instance.Raise(new PausePlayerEvent());
    }

    public void Resume(GameResumeEvent e)
    {
        m_PausePanel.SetActive(false);
        EventManager.Instance.Raise(new ResumePlayerEvent());
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.P))
        {
            if (m_CanClick)
            {
                PauseButtonnClicked();
                m_CanClick = false;
            }
        }
        else if (!m_CanClick)
        {
            m_CanClick = true;
        }

    }

    public void ReturnMenuButtonClicked()
    {
        EventManager.Instance.Raise(new ReturnMenuButtonClickedEvent());
    }
}
