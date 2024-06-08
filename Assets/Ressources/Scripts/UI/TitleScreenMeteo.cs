using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TitleScreenMeteo : MonoBehaviour
{
    [SerializeField] private CloudsPostProcess clouds;
    [SerializeField] private AnimationCurve coverageCurve;
    [SerializeField] private float coverageDuration;

    private void Update()
    {
        DateTime time = DateTime.Now;
        float seconds = time.Hour * 3600f + time.Minute * 60f + time.Second + time.Millisecond / 1000f;
        float t = (seconds % coverageDuration) / coverageDuration;
        clouds.Coverage = Mathf.Clamp01(coverageCurve.Evaluate(t));
    }
}
