using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualCameraManager : MonoBehaviour
{
    public Camera cam1;
    public Camera cam2;

    public Transform player1;
    public Transform player2;

    public PlayerDualCamera PlayCam1;
    public PlayerDualCamera PlayCam2;

    private void Start()
    {
        PlayCam1.target = player1;
        PlayCam2.target = player2;

        cam1.rect = new Rect(0f, 0f, 0.5f, 1f);
        cam2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    }
}
