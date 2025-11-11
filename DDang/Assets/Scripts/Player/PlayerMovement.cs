using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public enum PlayerState
{
    Idle,           //기본 상태
    Controllable,   //조작 가능
    Attacking,      //공격 중
    Stunned,        //기절 상태

}

public class PlayerMovement : MonoBehaviour
{
    public float minValue = 0.1f;
    public float moveSpeed = 5.0f;
    public PlayerInput playerInput;
    public LayerMask mask;
    public float stunTime = 1.5f;

    public PlayerType playerType;

    private float attackRange = 0.5f;
    private float attackCooldown = 0.5f;

    private Rigidbody rb;

    private Transform playerTransform;
    private Vector3 moveDirection;
    private PlayerState currentState;

    private StoreTile currentStoreTile;

    public PlayerState state { get; private set; } = PlayerState.Controllable;

    private void Start()
    {
        playerTransform = transform;
        playerInput = GetComponent<PlayerInput>();

        rb = GetComponent<Rigidbody>();

        if (RoundManager.Instance != null && RoundManager.Instance.currentState == RoundState.WaitingRound)
        {
            state = PlayerState.Idle;
        }
    }

    private void Update()
    {
        PlayerStates(state);
    }


    void PlayerStates(PlayerState states)
    {
        switch (states)
        {
            case PlayerState.Idle:                                  //대기 상태
                rb.velocity = Vector3.zero;

                if (RoundManager.Instance != null && RoundManager.Instance.currentState == RoundState.Playing)
                {
                    state = PlayerState.Controllable;
                }
                break;
            
            case PlayerState.Controllable:                          //조작 상태
                rb.velocity = moveDirection * moveSpeed;

                if (moveDirection != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
                }
                break;

            case PlayerState.Attacking:                             //공격 중
                
                if (currentStoreTile != null && RoundManager.Instance.currentState == RoundState.Store)
                {
                    return;
                }
            
                if (state == PlayerState.Controllable)
                {
                    rb.velocity = Vector3.zero;
                }
                break;

            case PlayerState.Stunned:                               //기절 중
                rb.velocity = Vector3.zero;
                break;
        }
    }

    public void OnMove(InputAction.CallbackContext context)                     //인풋 시스템 이동로직
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

    public void OnAttack(InputAction.CallbackContext context)                  //인풋 시스템 공격로직
    {
        if (state == PlayerState.Attacking) return;             //이미 공격 상태이면 반환

        if (context.performed)
        {

            if (currentStoreTile != null && RoundManager.Instance.currentState == RoundState.Store)
            {
                currentStoreTile.TryPurchase(this);
                return;
            }

            if (state == PlayerState.Controllable)
            {
                state = PlayerState.Attacking;                      //조작 => 공격으로 상태전환
                ChaeckAttackRange();                                //공격 사거리 체크 매서드
                StartCoroutine(AttackCooldown(attackCooldown));     //공격 쿨다운 코루틴 실행
            }
        }
    }

    public void ChaeckAttackRange()
    {
        Vector3 checkPosition = playerTransform.position + playerTransform.forward * (attackRange * 0.5f);

        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, attackRange, mask);                 //구체와 충돌한 모든 콜라이더 배열

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject == this.gameObject) continue;                //자기 제외

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
        if (state == PlayerState.Stunned) return;               //이미 기절상태면 반환

        Debug.Log("기절 됨!");
        state = PlayerState.Stunned;                            //기정 상태
        rb.velocity = Vector3.zero;                             //움직임 정지

        StartCoroutine(PlayerStun(duration));
    }

    IEnumerator PlayerStun(float duration)
    {
        yield return new WaitForSeconds(duration);              //duration만큼 기다리기
        state = PlayerState.Controllable;                       //시간이 지나면 조작 가능상태로 변경
    }

    IEnumerator AttackCooldown(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (state == PlayerState.Attacking)
            state = PlayerState.Controllable;
    }

    private void OnTriggerEnter(Collider other)
    {
        StoreTile tile = other.GetComponent<StoreTile>();

        if (tile != null && currentStoreTile == tile)
        {
            currentStoreTile = null;
        }
    }
}
