using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var p = other.GetComponent<Adrift.Game.PlayerController>();
        if (p)
        {
            FindObjectOfType<Adrift.Game.GameManager>().EndGame();
            Debug.Log("Chicken dinner....");
        }
    }
}
