using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;

public class PlayerManager : MonoBehaviour, IEventHandler
{
    [SerializeField]
    List<GameObject> m_PlayerObjects;
    [SerializeField]
    GameObject blackScreen;
    [SerializeField]
    GameObject particleCam;


    public void Start()
    {
        SetActiveObjects(false);
    }

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GamePlayStartEvent>(EnablePlayer);
        EventManager.Instance.AddListener<PausePlayerEvent>(PausePlayer);
        EventManager.Instance.AddListener<ResumePlayerEvent>(ResumePlayer);
        EventManager.Instance.AddListener<DestroyEvent>(Destroy);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GamePlayStartEvent>(EnablePlayer);
        EventManager.Instance.RemoveListener<PausePlayerEvent>(PausePlayer);
        EventManager.Instance.RemoveListener<ResumePlayerEvent>(ResumePlayer);
        EventManager.Instance.RemoveListener<DestroyEvent>(Destroy);

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

    void EnablePlayer(GamePlayStartEvent e)
    {
        SetActiveObjects(true);
        blackScreen?.SetActive(false);
        EventManager.Instance.Raise(new RequestWorldGeneratorEvent());
    }

    void PausePlayer(PausePlayerEvent e)
    {
        SetActiveObjects(false);
    }

    void ResumePlayer(ResumePlayerEvent e)
    {
        SetActiveObjects(true);
    }

    void Destroy(DestroyEvent e)
    {
        EventManager.Instance.Raise(new ExplosionEvent());
        SetActiveObjects(false);
        StartCoroutine(WaitExplosion());
    }

    private IEnumerator WaitExplosion()
    {
        yield return new WaitForSeconds(1);
        blackScreen?.SetActive(true);
        EventManager.Instance.Raise(new PlayerExplosedEvent());
    }

}