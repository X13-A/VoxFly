using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoOnGameOver : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<GameOverEvent>(PlayVideo);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<GameOverEvent>(PlayVideo);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void Start()
    {
        videoPlayer.loopPointReached += EndReached;
    }

    void PlayVideo(GameOverEvent e)
    {
        if (videoPlayer != null && videoPlayer.clip != null)
        {
            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                if (obj.name == "Rendering" || obj.name == "HUD")
                {
                    obj.SetActive(false);
                }
            }   
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("Video player or video clip not set correctly");
        }
    }

    void EndReached(VideoPlayer vp)
    {
        Debug.Log("Video ended");
    }
}
