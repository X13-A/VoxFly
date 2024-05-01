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

    //int m_Score;
    //[SerializeField] int m_ScoreToVictory;

    /*void SetScore(int newScore)
    {
        m_Score = newScore;
        EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eScore = m_Score, eCountDown = m_CountDown });
    }*/

    /*int IncrementScore(int increment)
    {
        SetScore(m_Score + increment);
        return m_Score;
    }*/

    float m_CountDown;
    [SerializeField] float m_GameDuration;
    /*void SetCountdown(float newCountdown)
    {
        m_CountDown = newCountdown;
        EventManager.Instance.Raise(new GameStatisticsChangedEvent() { eScore = m_Score, eCountDown = m_CountDown });
    }*/

    /*float DecrementCountdown(float decrement)
    {
        SetCountdown(Mathf.Max(0, m_CountDown - decrement));
        return m_CountDown;
    }*/

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<PlayButtonClickedEvent>(PlayButtonClicked);
        EventManager.Instance.AddListener<ReplayButtonClickedEvent>(ReplayButtonClicked);
        EventManager.Instance.AddListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
        EventManager.Instance.AddListener<QuitButtonClickedEvent>(QuitButtonClicked);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<PlayButtonClickedEvent>(PlayButtonClicked);
        EventManager.Instance.RemoveListener<ReplayButtonClickedEvent>(ReplayButtonClicked);
        EventManager.Instance.RemoveListener<MainMenuButtonClickedEvent>(MainMenuButtonClicked);
        EventManager.Instance.RemoveListener<QuitButtonClickedEvent>(QuitButtonClicked);
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
        //else Destroy(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        //SetScore(0);
        //SetCountdown(0);
        MainMenu();
    }

    void Update()
    {
        /*if (IsPlaying)
        {
            if (DecrementCountdown(Time.deltaTime) == 0)
                GameOver();
        }*/
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
        //SetScore(0);
        //SetCountdown(m_GameDuration);
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
        SetState(GAMESTATE.gameover);
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

    // Ball events' callbacks
    /*void EnemyHasBeenHit(EnemyHasBeenHitEvent e)
    {
        IScore score = e.eEnemy.GetComponent<IScore>();
        if (null != score
            && IncrementScore(score.Score) >= m_ScoreToVictory)
            Victory();
    }*/

}
