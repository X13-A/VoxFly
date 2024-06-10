using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class PauseManager : MonoBehaviour, IEventHandler
{

    private bool available = false;
    private bool m_CanClick = true;
    [SerializeField] GameObject m_PausePanel;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GamePauseEvent>(Pause);
        EventManager.Instance.AddListener<GameResumeEvent>(Resume);
        EventManager.Instance.AddListener<GamePlayStartEvent>(SetAvailable);
        EventManager.Instance.AddListener<DestroyEvent>(SetUnavailable);
        EventManager.Instance.AddListener<CreativeModeStartEvent>(SetAvailable);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GamePauseEvent>(Pause);
        EventManager.Instance.RemoveListener<GameResumeEvent>(Resume);
        EventManager.Instance.RemoveListener<GamePlayStartEvent>(SetAvailable);
        EventManager.Instance.RemoveListener<DestroyEvent>(SetUnavailable);
        EventManager.Instance.RemoveListener<CreativeModeStartEvent>(SetAvailable);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }
    void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void SetAvailable(GamePlayStartEvent e)
    {
        available = true;
    }

    private void SetAvailable(CreativeModeStartEvent e)
    {
        available = true;
    }

    private void SetUnavailable(DestroyEvent e)
    {
        available = false;
    }

    public void PauseButtonClicked()
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
        if (Input.GetKey(KeyCode.Escape))
        {
            if (m_CanClick && available)
            {
                PauseButtonClicked();
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

    public void ResumeButtonClicked()
    {
        EventManager.Instance.Raise(new PauseButtonClickedEvent());
    }
}
