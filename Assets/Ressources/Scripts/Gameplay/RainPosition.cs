using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainPosition : MonoBehaviour
{
    [SerializeField] private GameObject player;

    void Update()
    {
        gameObject.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 10, player.transform.position.z);
    }
}
