using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public enum PlayerState
{
    Idle,           //기본 상태
    Controllable,   //조작 가능
}

public class PlayerMovement : MonoBehaviour
{
    public float minValue = 0.1f;
    public float moveSpeed = 5.0f;
    public PlayerInput playerInput;

    private Rigidbody rb;

    private Vector3 moveDirection;

    public PlayerState state { get; private set; } = PlayerState.Controllable;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        rb = GetComponent<Rigidbody>();

    }

    private void FixedUpdate()
    {
        if (state != PlayerState.Controllable)
        {
            rb.velocity = Vector3.zero;
        }
        else
        {
            rb.velocity = moveDirection * moveSpeed;

            if (moveDirection != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            }
        }

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector3 forward = Vector3.left;
        Vector3 right = Vector3.forward;

        forward.y = 0;
        right.y = 0;

        Vector3 direction = forward * context.ReadValue<Vector2>().y + right * context.ReadValue<Vector2>().x;
        if (direction.magnitude > minValue)
        {
            moveDirection = direction.normalized;
        }
        else
        {
            moveDirection = Vector3.zero; 
        }
    }
}
