using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDD.Events;
using TMPro;

public class ScoreManager : MonoBehaviour, IEventHandler
{
    [SerializeField]
    int nScores;
    [SerializeField]
    TMP_Text scoresText;

    public void SubscribeEvents()
    {
        EventManager.Instance.AddListener<UpdateScoreEvent>(UpdateScores);
        EventManager.Instance.AddListener<UpdateScoresTextEvent>(UpdateScoresText);
    }

    public void UnsubscribeEvents()
    {
        EventManager.Instance.RemoveListener<UpdateScoreEvent>(UpdateScores);
        EventManager.Instance.RemoveListener<UpdateScoresTextEvent>(UpdateScoresText);
    }

    void OnEnable()
    {
        SubscribeEvents();
    }
    void OnDisable()
    {
        UnsubscribeEvents();
    }

    List<int> ReadScores()
    {
        string scoresString = PlayerPrefs.GetString("PlayerScores", "");
        List<int> scores = new List<int>();

        if (string.IsNullOrEmpty(scoresString))
        {
            return scores;
        }
        
        foreach (var score in scoresString.Split(','))
        {
            if (int.TryParse(score, out int result))
            {
                scores.Add(result);
            }
        }
        return scores;
    }

    void UpdateScores(UpdateScoreEvent e)
    {
        List<int> scores = ReadScores();
        scores.Add(e.score);
        scores.Sort((a, b) => b.CompareTo(a));
        if (scores.Count > nScores)
        {
            scores = scores.GetRange(0, nScores);
        }
        string scoresString = string.Join(",", scores);
        PlayerPrefs.SetString("PlayerScores", scoresString);
        PlayerPrefs.Save();
    }

    void UpdateScoresText(UpdateScoresTextEvent e)
    {
        string text = "";
        int i = 1;
        List<int> scores = ReadScores();

        foreach (var score in scores)
        {
            text += i + " : " + score + "\n\n";
            i++;
        }
        scoresText.text = text;
    }
}
