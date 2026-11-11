using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cameras;

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
        PlayCam1.SetTarget(player1);                // 시작 시 카메라가 플레이어 위치로 바로 이동
        PlayCam2.SetTarget(player2);

        cam1.rect = new Rect(0f, 0f, 0.5f, 1f);
        cam2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    }
}
