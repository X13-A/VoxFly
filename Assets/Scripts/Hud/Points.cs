using SDD.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Points : MonoBehaviour, IEventHandler
{
    [SerializeField] TextMeshProUGUI pointsText;
    [SerializeField] float duration = 1.5f;
    [SerializeField] float moveUpAmount = 50f;
    [SerializeField] float scaleAmount = 1.2f;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<WinningPointsEvent>(AnimatePoints);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<WinningPointsEvent>(AnimatePoints);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }

    void OnDisable()
    {
        UnsubscribeEvents();
    }

    void AnimatePoints(WinningPointsEvent e)
    {
        Debug.Log("Points: " + e.ePoints);
        pointsText.text = "+" + e.ePoints;
        pointsText.gameObject.SetActive(true);
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        float timer = 0;
        Vector3 originalPosition = pointsText.transform.position;
        Vector3 targetPosition = originalPosition + new Vector3(0, moveUpAmount, 0);
        Vector3 originalScale = pointsText.transform.localScale;
        Vector3 targetScale = originalScale * scaleAmount;

        while (timer < duration)
        {
            float t = timer / duration;
            pointsText.transform.position = Vector3.Lerp(originalPosition, targetPosition, t);
            pointsText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            timer += Time.deltaTime;
            yield return null;
        }

        pointsText.gameObject.SetActive(false);
        pointsText.transform.position = originalPosition;
        pointsText.transform.localScale = originalScale;
    }
}
