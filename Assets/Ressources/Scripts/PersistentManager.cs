using UnityEngine;

public class PersistentManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);  // Emp�che la destruction de cet objet lors du chargement des sc�nes
    }
}