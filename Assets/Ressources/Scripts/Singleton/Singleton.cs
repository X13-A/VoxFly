using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    static T _instance;
    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError($"An instance of {typeof(T).Name} already exists. Deleting the new one.");
            Destroy(gameObject);
        }
        else _instance = this as T;
    }
}