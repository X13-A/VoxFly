using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;

public class LosePanel : MonoBehaviour
{
    [SerializeField]
    GameObject loseObjects;
    [SerializeField]
    TMP_Text scoreText;
    [SerializeField]
    VideoPlayer video;

    void OnEnable()
    {
        StartCoroutine(ActivateAfterDelay());
    }
    void OnDisable()
    {
        loseObjects.SetActive(false);
    }

    private IEnumerator ActivateAfterDelay()
    {
        video.gameObject.SetActive(true);
        video.Stop();
        video.Play();
        yield return new WaitForSeconds(7);
        video.Stop();
        video.gameObject.SetActive(false);
        loseObjects.SetActive(true);
        scoreText.text = string.Format("Final Score : {0:0}", GameManager.Instance.Score);
    }
}
