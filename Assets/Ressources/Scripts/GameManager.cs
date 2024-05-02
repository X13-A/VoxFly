using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using UnityEngine.SceneManagement;

public enum GAMESTATE { menu, play, pause, victory, gameover }

public class GameManager : MonoBehaviour, IEventHandler
{
    private static GameManager m_Instance;
    public static GameManager Instance { get { return m_Instance; } }

    GAMESTATE m_State;
    public bool IsPlaying => m_State == GAMESTATE.play;

    public int m_Score;

    void SetScore(int newScore)
    {
        m_Score = newScore;
        EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eScore = m_Score });
    }

    public int IncrementScore(int increment)
    {
        SetScore(m_Score + increment);
        return m_Score;
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlayButtonClickedEvent>(PlayButtonClicked);
        EventManager.Instance.AddListener<ReplayButtonClickedEvent>(ReplayButtonClicked);
        EventManager.Instance.AddListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
        EventManager.Instance.AddListener<QuitButtonClickedEvent>(QuitButtonClicked);
        EventManager.Instance.AddListener<SettingsButtonClickedEvent>(SettingsButtonClicked);
        EventManager.Instance.AddListener<ScoreButtonClickedEvent>(ScoreButtonClicked);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlayButtonClickedEvent>(PlayButtonClicked);
        EventManager.Instance.RemoveListener<ReplayButtonClickedEvent>(ReplayButtonClicked);
        EventManager.Instance.RemoveListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
        EventManager.Instance.RemoveListener<QuitButtonClickedEvent>(QuitButtonClicked);
        EventManager.Instance.RemoveListener<SettingsButtonClickedEvent>(SettingsButtonClicked);
        EventManager.Instance.RemoveListener<ScoreButtonClickedEvent>(ScoreButtonClicked);
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
        if (!m_Instance) m_Instance = this;
    }

    void Start()
    {
        SetScore(0);
        MainMenu();
    }

    void SetState(GAMESTATE newState)
    {
        m_State = newState;

        switch (m_State)
        {
            case GAMESTATE.menu:
                EventManager.Instance.Raise(new GameMenuEvent());
                break;
            case GAMESTATE.play:
                EventManager.Instance.Raise(new GamePlayEvent());
                break;
            case GAMESTATE.victory:
                EventManager.Instance.Raise(new GameVictoryEvent());
                break;
            case GAMESTATE.gameover:
                EventManager.Instance.Raise(new GameOverEvent());
                break;
        }

    }

    void MainMenu()
    {
        SetState(GAMESTATE.menu);
    }

    void InitGame()
    {
        SceneManager.LoadSceneAsync(1);
        SetScore(0);
    }

    void Play()
    {
        InitGame();
        SetState(GAMESTATE.play);
    }

    void Victory()
    {
        SetState(GAMESTATE.victory);
    }
    void GameOver()
    {
        UpdateScores();
        SetState(GAMESTATE.gameover);
    }

    void UpdateScores()
    {
        EventManager.Instance.Raise(new UpdateScoreEvent(m_Score));
    }

    // MenuManager events' callback
    void PlayButtonClicked(PlayButtonClickedEvent e)
    {
        Play();
    }

    void ReplayButtonClicked(ReplayButtonClickedEvent e)
    {
        Play();
    }
    void MainMenuButtonClicked(MainMenuButtonClickedEvent e)
    {
        MainMenu();
    }

    void QuitButtonClicked(QuitButtonClickedEvent e)
    {
        Application.Quit();
    }

    void SettingsButtonClicked(SettingsButtonClickedEvent e)
    {
        EventManager.Instance.Raise(new GameSettingsEvent());
    }

    void ScoreButtonClicked(ScoreButtonClickedEvent e)
    {
        EventManager.Instance.Raise(new GameScoreEvent());
    }
}
