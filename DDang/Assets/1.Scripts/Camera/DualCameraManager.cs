using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cameras;
using Online;

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

        if (OnlineSession.IsOnline)                 // 온라인: 내 토끼 카메라만 전체 화면
        {
            bool p1 = OnlineSession.Instance.MyPlayer == PlayerType.Player1;
            (p1 ? cam1 : cam2).rect = new Rect(0f, 0f, 1f, 1f);
            (p1 ? cam2 : cam1).gameObject.SetActive(false);
            return;
        }

        cam1.rect = new Rect(0f, 0f, 0.5f, 1f);
        cam2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    }
}
