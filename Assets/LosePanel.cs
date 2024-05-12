using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LosePanel : MonoBehaviour
{
    [SerializeField]
    GameObject loseObjects;
    [SerializeField]
    TMP_Text scoreText;

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
        yield return new WaitForSeconds(7);
        loseObjects.SetActive(true);
        scoreText.text = string.Format("Final Score : {0:0}", GameManager.Instance.Score);
    }
}
