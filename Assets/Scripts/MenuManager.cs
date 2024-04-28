using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class MenuManager : MonoBehaviour, IEventHandler
{
    [SerializeField] GameObject m_MainMenuPanel;
    [SerializeField] GameObject m_VictoryPanel;
    [SerializeField] GameObject m_GameOverPanel;

    List<GameObject> m_Panels;
    void OpenPanel(GameObject panel)
    {
        m_Panels.ForEach(item => item.SetActive(panel == item));
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GameMenuEvent>(GameMenu);
        EventManager.Instance.AddListener<GamePlayEvent>(GamePlay);
        EventManager.Instance.AddListener<GameVictoryEvent>(GameVictory);
        EventManager.Instance.AddListener<GameOverEvent>(GameOver);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GameMenuEvent>(GameMenu);
        EventManager.Instance.RemoveListener<GamePlayEvent>(GamePlay);
        EventManager.Instance.RemoveListener<GameVictoryEvent>(GameVictory);
        EventManager.Instance.RemoveListener<GameOverEvent>(GameOver);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }
    void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Awake()
    {
        m_Panels = new List<GameObject>(
            new GameObject[] { m_MainMenuPanel, m_VictoryPanel, m_GameOverPanel });
    }

    // GameManager events' callbacks
    void GameMenu(GameMenuEvent e)
    {
        OpenPanel(m_MainMenuPanel);
    }

    void GamePlay(GamePlayEvent e)
    {
        OpenPanel(null);
    }

    void GameVictory(GameVictoryEvent e)
    {
        OpenPanel(m_VictoryPanel);
    }

    void GameOver(GameOverEvent e)
    {
        OpenPanel(m_GameOverPanel);
    }


    // UI events' callbacks
    public void PlayButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new PlayButtonClickedEvent());
    }
    public void ReplayButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new ReplayButtonClickedEvent());
    }
    public void MainMenuButtonHasBeenClicked()
    {
        EventManager.Instance.Raise(new MainMenuButtonClickedEvent());
    }
}
