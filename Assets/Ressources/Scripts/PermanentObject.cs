using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A permanent object is not destroyed when a new scene loads.
/// It can still be manually destroyed.
/// </summary>
public class PermanentObject : Singleton<PermanentObject>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
