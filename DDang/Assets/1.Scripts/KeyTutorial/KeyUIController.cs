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

    [Header("Player2")]
    public KeyUI keyUp;
    public KeyUI keyLeft;
    public KeyUI keyDown;
    public KeyUI keyRight;
    public KeyUI keyRShift;

    private void Update()
    {
        Player1Key();                                           // WASD + Space
        if (!Online.OnlineSession.IsOnline) Player2Key();       // 로컬만 2P 키 안내
    }

    private void Player1Key()
    {
        keyW.SetHeld(Input.GetKey(KeyCode.W));                  // 누르고 있는 동안 눌린 모습 유지
        keyA.SetHeld(Input.GetKey(KeyCode.A));
        keyS.SetHeld(Input.GetKey(KeyCode.S));
        keyD.SetHeld(Input.GetKey(KeyCode.D));
        keySpace.SetHeld(Input.GetKey(KeyCode.Space));
    }

    private void Player2Key()
    {
        keyUp.SetHeld(Input.GetKey(KeyCode.UpArrow));
        keyLeft.SetHeld(Input.GetKey(KeyCode.LeftArrow));
        keyDown.SetHeld(Input.GetKey(KeyCode.DownArrow));
        keyRight.SetHeld(Input.GetKey(KeyCode.RightArrow));
        keyRShift.SetHeld(Input.GetKey(KeyCode.RightShift));
    }
}
