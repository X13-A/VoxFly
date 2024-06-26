using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using UnityEngine.SceneManagement;

public enum GAMESTATE { menu, play, pause, victory, gameover }
public delegate void afterFunction();

public class GameManager : Singleton<GameManager>, IEventHandler
{
    GAMESTATE m_State;
    public bool IsPlaying => m_State == GAMESTATE.play;

    public int m_Score;
    public int Score {  get { return m_Score; } }
    void SetScore(int newScore)
    {
        m_Score = newScore;
        EventManager.Instance.Raise(new UpdateGameScoreEvent() { score = m_Score });
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
        EventManager.Instance.AddListener<PlayerExplodedEvent>(PlayerExploded);
        EventManager.Instance.AddListener<PauseButtonClickedEvent>(PauseButtonClicked);
        EventManager.Instance.AddListener<FinishTimerEvent>(FinishTimer);
        EventManager.Instance.AddListener<ReturnMenuButtonClickedEvent>(ReturnMenuButtonClicked);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlayButtonClickedEvent>(PlayButtonClicked);
        EventManager.Instance.RemoveListener<ReplayButtonClickedEvent>(ReplayButtonClicked);
        EventManager.Instance.RemoveListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
        EventManager.Instance.RemoveListener<QuitButtonClickedEvent>(QuitButtonClicked);
        EventManager.Instance.RemoveListener<SettingsButtonClickedEvent>(SettingsButtonClicked);
        EventManager.Instance.RemoveListener<ScoreButtonClickedEvent>(ScoreButtonClicked);
        EventManager.Instance.RemoveListener<PlayerExplodedEvent>(PlayerExploded);
        EventManager.Instance.RemoveListener<PauseButtonClickedEvent>(PauseButtonClicked);
        EventManager.Instance.RemoveListener<FinishTimerEvent>(FinishTimer);
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
        EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "menu", eLoop = true });
        SetState(GAMESTATE.menu);
    }

    void InitGame()
    {
        AsyncOperation loading = SceneManager.LoadSceneAsync(1);
        loading.completed += (AsyncOperation operation) => 
        {
            EventManager.Instance.Raise(new SceneLoadedEvent { scene = 1 });
            SetState(GAMESTATE.play);
        };
        SetScore(0);
    }

    void StartGame()
    {
        EventManager.Instance.Raise(new GamePlayStartEvent());
    }

    void Play()
    {
        InitGame();
        EventManager.Instance.Raise(new StopSoundEvent() { eNameClip = "menu" });
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
        EventManager.Instance.Raise(new UpdateScoreEvent { score = m_Score });
    }

    void Pause()
    {
        EventManager.Instance.Raise(new PauseAllSoundEvent() { ePause = true });
        EventManager.Instance.Raise(new GamePauseEvent());
        SetState(GAMESTATE.pause);
    }

    void Resume()
    {
        EventManager.Instance.Raise(new PauseAllSoundEvent() { ePause = false });
        EventManager.Instance.Raise(new GameResumeEvent());
        SetState(GAMESTATE.play);
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
        if (e.eResetMusic)
        {
            EventManager.Instance.Raise(new PlaySoundEvent() { eNameClip = "menu", eLoop = true });
        }
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

    void PauseButtonClicked(PauseButtonClickedEvent e)
    {
        if (m_State == GAMESTATE.pause)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    void ReturnMenuButtonClicked(ReturnMenuButtonClickedEvent e)
    {
        StartCoroutine(LoadSceneThenFunction(0, MainMenu));
    }

    void FinishTimer(FinishTimerEvent e)
    {
        StartGame();
    }

    void PlayerExploded(PlayerExplodedEvent e)
    {
        StartCoroutine(LoadSceneThenFunction(0, GameOver));
    }

    // Coroutine pour charger la sc�ne de mani�re asynchrone et appeler la fonction sp�cifi�e
    private IEnumerator LoadSceneThenFunction(int sceneIndex, afterFunction function)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);
        // Attendre que la sc�ne soit charg�e
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        // La sc�ne est charg�e, appeler la fonction sp�cifi�e
        function.Invoke();
    }
}
