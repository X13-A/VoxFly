using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class PauseManager : MonoBehaviour, IEventHandler
{

    private bool ready = false;
    private bool m_CanClick = true;
    [SerializeField] GameObject m_PausePanel;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GamePauseEvent>(Pause);
        EventManager.Instance.AddListener<GameResumeEvent>(Resume);
        EventManager.Instance.AddListener<FinishTimerEvent>(SetReady);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GamePauseEvent>(Pause);
        EventManager.Instance.RemoveListener<GameResumeEvent>(Resume);
        EventManager.Instance.RemoveListener<FinishTimerEvent>(SetReady);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }
    void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SetReady(FinishTimerEvent e)
    {
        ready = true;
    }

    public void PauseButtonnClicked()
    {
        EventManager.Instance.Raise(new PauseButtonClickedEvent());
    }

    public void Pause(GamePauseEvent e)
    {
        m_PausePanel.SetActive(true);
        EventManager.Instance.Raise(new PausePlayerEvent());
        EventManager.Instance.Raise(new MuteAllSoundEvent() { eMute = true });
    }

    public void Resume(GameResumeEvent e)
    {
        m_PausePanel.SetActive(false);
        EventManager.Instance.Raise(new ResumePlayerEvent());
        EventManager.Instance.Raise(new MuteAllSoundEvent() { eMute = false });
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.P))
        {
            if (m_CanClick && ready)
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
