using UnityEngine;

public class PersistentManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);  // Empêche la destruction de cet objet lors du chargement des scènes
    }
}