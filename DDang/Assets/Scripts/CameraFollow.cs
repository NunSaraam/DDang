using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraFollow : MonoBehaviour
{
    public Camera mainCamera;
    public Camera player1Camera;
    public Camera player2Camera;

    public Transform target = null;

    public float smoothSpeed = 0.125f;

    public Vector3 offset = new Vector3(4, 10 , 0);

    private void Start()
    {
        player1Camera.enabled = false;
        player2Camera.enabled = false;

        //카메라 씬 나누기(플레이어별 카메라)
        /*
        mainCamera.enabled = false;
        player1Camera.rect = new Rect(0f, 0f, 0.5f, 1f);
        player2Camera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        */
    }

    private void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }
}
