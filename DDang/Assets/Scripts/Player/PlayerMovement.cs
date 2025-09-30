using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public enum PlayerState
{
    Idle,           //�⺻ ����
    Controllable,   //���� ����
    Attacking,      //���� ��
    Stunned         //���� ����
}

public class PlayerMovement : MonoBehaviour
{
    public float minValue = 0.1f;
    public float moveSpeed = 5.0f;
    public PlayerInput playerInput;
    public LayerMask mask;
    public float stunTime = 1.5f;

    private float attackRange = 0.5f;
    private float attackCooldown = 0.5f;

    private Rigidbody rb;

    private Transform playerTransform;
    private Vector3 moveDirection;

    public PlayerState state { get; private set; } = PlayerState.Controllable;

    private void Start()
    {
        playerTransform = transform;
        playerInput = GetComponent<PlayerInput>();

        rb = GetComponent<Rigidbody>();

    }

    private void Update()
    {
        PlayerStates(state);
    }

    void PlayerStates(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:                                  //��� ����
                rb.velocity = Vector3.zero;
                break;
            
            case PlayerState.Controllable:                          //���� ����
                rb.velocity = moveDirection * moveSpeed;

                if (moveDirection != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
                }
                break;

            case PlayerState.Attacking:                             //���� ��
                rb.velocity = Vector3.zero;
                break;

            case PlayerState.Stunned:                               //���� ��
                rb.velocity = Vector3.zero;
                break;
        }
    }

    public void OnMove(InputAction.CallbackContext context)                     //��ǲ �ý��� �̵�����
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

    public void OnAttack(InputAction.CallbackContext context)                  //��ǲ �ý��� ���ݷ���
    {
        if (context.performed && state == PlayerState.Controllable)
        {
            state = PlayerState.Attacking;              //���� => �������� ������ȯ
            ChaeckAttackRange();                                //���� ��Ÿ� üũ �ż���
            StartCoroutine(AttackCooldown(attackCooldown));     //���� ��ٿ� �ڷ�ƾ ����
        }
    }

    public void ChaeckAttackRange()
    {
        Vector3 checkPosition = playerTransform.position + playerTransform.forward * (attackRange * 0.5f);

        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, attackRange, mask);                 //��ü�� �浹�� ��� �ݶ��̴� �迭

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject == this.gameObject) continue;                //�ڱ� ����

            PlayerMovement target = collider.GetComponent<PlayerMovement>();
            if (target != null)
            {
                Vector3 targetDirection = (collider.transform.position - playerTransform.position).normalized;
                float angle = Vector3.Angle(transform.forward, targetDirection);

                if (angle < 90f)
                {
                    target.Stun(stunTime);
                }
            }
            
        }
         
    }

    void Stun(float duration)
    {
        if (state == PlayerState.Stunned) return;               //�̹� �������¸� ��ȯ

        Debug.Log("���� ��!");
        state = PlayerState.Stunned;                            //���� ����
        rb.velocity = Vector3.zero;                             //������ ����

        StartCoroutine(PlayerStun(duration));
    }

    IEnumerator PlayerStun(float duration)
    {
        yield return new WaitForSeconds(duration);              //duration��ŭ ��ٸ���
        state = PlayerState.Controllable;                       //�ð��� ������ ���� ���ɻ��·� ����
    }

    IEnumerator AttackCooldown(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (state == PlayerState.Attacking)
            state = PlayerState.Controllable;
    }
}
