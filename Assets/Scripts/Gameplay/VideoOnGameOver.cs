using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoOnGameOver : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;

    bool flag = false;
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
            if (!flag) CreateTextMeshPro();
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

    void CreateTextMeshPro()
    {
        flag = true;
        Canvas canvas = new GameObject("OverlayCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>();
        canvas.gameObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject("GameOverText");
        textObject.transform.SetParent(canvas.transform, false);

        TextMeshProUGUI textMesh = textObject.AddComponent<TextMeshProUGUI>();
        textMesh.text = "Your final score: " + GameManager.Instance.m_Score;
        textMesh.fontSize = 30;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = Color.white;
        textMesh.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Roboto-Bold SDF");

        RectTransform rectTransform = textMesh.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1);
        rectTransform.anchorMax = new Vector2(0.5f, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.sizeDelta = new Vector2(600, 100);
        rectTransform.anchoredPosition = new Vector2(0, -20); // Position just at the top of the screen
    }
}
