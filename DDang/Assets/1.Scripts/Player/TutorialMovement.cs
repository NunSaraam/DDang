using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialMovement : MonoBehaviour
{
    public float minValue = 0.1f;
    public float moveSpeed = 5.0f;
    public Animator animator;

    private float attackCooldown = 0.4f;
    private float attackRange = 0.6f;

    private Vector3 moveDirection;
    private Rigidbody rb;
    private Transform playerTransform;

    public PlayerState state { get; private set; } = PlayerState.Controllable;

    private void Start()
    {
        playerTransform = transform;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        PlayerStates();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        Vector3 direction = (forward * input.y + right * input.x);

        if (direction.magnitude > minValue)
        {
            moveDirection = direction.normalized;
            animator?.SetFloat("Move", 1f);
        }
        else
        {
            moveDirection = Vector3.zero;
            animator?.SetFloat("Move", 0f);
        }
    }

    void PlayerStates()
    {
        switch (state)
        {
            case PlayerState.Controllable:
                rb.velocity = moveDirection * moveSpeed;

                if (moveDirection != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
                }
                break;

            case PlayerState.Attacking:
                rb.velocity = Vector3.zero;
                break;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (state == PlayerState.Attacking) return;

        // 공격 상태 진입
        state = PlayerState.Attacking;

        // 애니메이션 실행
        animator?.SetTrigger("Attack");

        // 튜토리얼 용: 공격 판정 없음 → 단순히 연출만 진행
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        state = PlayerState.Controllable;
    }
}
