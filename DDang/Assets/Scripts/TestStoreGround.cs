using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestStoreGround : MonoBehaviour
{
    public GameObject uiCanvas;

    public Transform player1;
    public Transform player2;

    public Outline line;

    private void Start()
    {
        line.enabled = false;   
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
        {
            line.enabled = true;

        }
    }
}
