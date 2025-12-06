using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeyUIController : MonoBehaviour
{
    [Header("Player1")]
    public KeyUI keyW;
    public KeyUI keyA;
    public KeyUI keyS;
    public KeyUI keyD;
    public KeyUI keySpace;

    [Header("Player1")]
    public KeyUI keyUp;
    public KeyUI keyLeft;
    public KeyUI keyDown;
    public KeyUI keyRight;
    public KeyUI keyRShift;

    private void Update()
    {
        Player1Key();
        Player2Key();
    }

    private void Player1Key()
    {
        if (Input.GetKey(KeyCode.W)) keyW.OnKeyPressed();
        if (Input.GetKey(KeyCode.A)) keyA.OnKeyPressed();
        if (Input.GetKey(KeyCode.S)) keyS.OnKeyPressed();
        if (Input.GetKey(KeyCode.D)) keyD.OnKeyPressed();
        if (Input.GetKey(KeyCode.Space)) keySpace.OnKeyPressed();
    }

    private void Player2Key()
    {
        if (Input.GetKey(KeyCode.UpArrow)) keyUp.OnKeyPressed();
        if (Input.GetKey(KeyCode.LeftArrow)) keyLeft.OnKeyPressed();
        if (Input.GetKey(KeyCode.DownArrow)) keyDown.OnKeyPressed();
        if (Input.GetKey(KeyCode.RightArrow)) keyRight.OnKeyPressed();
        if (Input.GetKey(KeyCode.RightShift)) keyRShift.OnKeyPressed();
    }
}
