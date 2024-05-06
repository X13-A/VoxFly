using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LosePanel : MonoBehaviour
{
    [SerializeField]
    GameObject loseObjects;
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
    }
}
