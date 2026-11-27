using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialMovement : MonoBehaviour
{
    public float minValue = 0.1f;
    public float moveSpeed = 5.0f;
    public Animator animator;
    [SerializeField] private bool sideView;     // 온라인 대기실: 인게임과 같은 옆 시점 방향 (화면 위 = 월드 -X)

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

    // 물리 주기에 맞춰 처리 (Update에서 transform을 바꾸면 Rigidbody 보간이 깨져 끊겨 보임)
    private void FixedUpdate()
    {
        PlayerStates();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();

        Vector3 forward = sideView ? Vector3.left : Vector3.forward;
        Vector3 right = sideView ? Vector3.forward : Vector3.right;

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
                rb.linearVelocity = moveDirection * moveSpeed;

                if (moveDirection != Vector3.zero)              // transform 직접 대입 대신 물리로 회전
                {
                    Quaternion rotation = Quaternion.LookRotation(moveDirection);
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, rotation, Time.fixedDeltaTime * 10f));
                }
                break;

            case PlayerState.Attacking:
                rb.linearVelocity = Vector3.zero;
                break;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (state == PlayerState.Attacking) return;

        // ���� ���� ����
        state = PlayerState.Attacking;

        // �ִϸ��̼� ����
        animator?.SetTrigger("Attack");
        if (Online.OnlineSession.IsOnline && TryGetComponent(out Online.OnlineLobbyRabbit net))
            net.PlayAttackForOthers();                          // 온라인 대기실: 상대 화면에도 공격 모션

        // Ʃ�丮�� ��: ���� ���� ���� �� �ܼ��� ���⸸ ����
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        state = PlayerState.Controllable;
    }
}
