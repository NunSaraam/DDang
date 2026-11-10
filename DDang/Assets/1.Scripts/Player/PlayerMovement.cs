using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using DDang.Rules;


public enum PlayerState
{
    Idle,
    Controllable,
    Attacking,
    Stunned,

}

public class PlayerMovement : MonoBehaviour
{
    public float minValue = 0.1f;
    public float moveSpeed = 5.0f;
    public PlayerInput playerInput;
    public LayerMask mask;
    public float stunTime = 1.5f;

    public PlayerType playerType;

    public Animator animator;

    private float attackRange = 0.5f;
    private float attackCooldown = 0.5f;

    private Rigidbody rb;

    private Transform playerTransform;
    private Vector3 moveDirection;
    private PlayerState currentState;

    public StoreTile currentStoreTile;
    [SerializeField] StoreUI curretnStoreUI;

    public PlayerState state { get; private set; } = PlayerState.Controllable;

    private void Start()
    {
        playerTransform = transform;
        playerInput = GetComponent<PlayerInput>();


        if (playerType == PlayerType.Player1)
        {
            moveSpeed = PlayerDataManager.Instance.p1MoveSpeed;
            stunTime = PlayerDataManager.Instance.p1StunTime;

        }
        else if (playerType == PlayerType.Player2)
        {
            moveSpeed = PlayerDataManager.Instance.p2MoveSpeed;
            stunTime = PlayerDataManager.Instance.p2StunTime;
        }

        rb = GetComponent<Rigidbody>();

        if (!CanMoveInPhase())
        {
            state = PlayerState.Idle;
        }
    }

    private void Update()
    {
        if (!CanMoveInPhase())
        {
            state = PlayerState.Idle;
        }
    }

    private void FixedUpdate()
    {
        PlayerStates(state);
    }

    // 라운드 진행 중이거나 상점에서만 이동 가능
    private bool CanMoveInPhase()
    {
        RoundManager round = RoundManager.Instance;
        return round.IsRoundActive || round.Phase == RoundPhase.Store;
    }


    void PlayerStates(PlayerState states)
    {
        switch (states)
        {
            case PlayerState.Idle:
                rb.linearVelocity = Vector3.zero;

                if (CanMoveInPhase())
                {
                    state = PlayerState.Controllable;
                }
                break;
            
            case PlayerState.Controllable:
                rb.linearVelocity = moveDirection * moveSpeed;

                if (moveDirection != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(moveDirection);
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, rotation, Time.fixedDeltaTime * 10f));
                }
                break;

            case PlayerState.Attacking:
                
                if (currentStoreTile != null && RoundManager.Instance.Phase == RoundPhase.Store)
                {
                    return;
                }
            
                if (state == PlayerState.Controllable)
                {
                    rb.linearVelocity = Vector3.zero;
                }
                break;

            case PlayerState.Stunned:
                rb.linearVelocity = Vector3.zero;
                break;
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

            if (animator != null)
            {
                animator.SetFloat("Move", 1f);
            }
        }
        else
        {
            moveDirection = Vector3.zero;

            if (animator != null)
            {
                animator.SetFloat("Move", 0f);
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (state == PlayerState.Attacking) return;

        if (context.performed)
        {

            if (RoundManager.Instance != null && RoundManager.Instance.Phase == RoundPhase.Store)
            {
                if (currentStoreTile != null)
                {
                    currentStoreTile.TryPurchase(this);
                    return;
                }
            }

            if (state == PlayerState.Controllable && RoundManager.Instance.IsRoundActive)
            {
                state = PlayerState.Attacking;
                ChaeckAttackRange();
                animator.SetTrigger("Attack");
                StartCoroutine(AttackCooldown(attackCooldown));
            }
        }
    }

    public void ChaeckAttackRange()
    {
        Vector3 checkPosition = playerTransform.position + playerTransform.forward * (attackRange * 0.5f);

        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, attackRange, mask);

        foreach (Collider collider in hitColliders)
        {
            if (collider.gameObject == this.gameObject) continue;

            PlayerMovement target = collider.GetComponent<PlayerMovement>();
            if (target != null)
            {
                Vector3 targetDirection = (collider.transform.position - playerTransform.position).normalized;
                float angle = Vector3.Angle(transform.forward, targetDirection);

                if (angle < 90f)
                {
                    target.Stun(stunTime);

                    QuestManager.Instance?.OnStunLanded(this.playerType);
                    RoundManager.Instance.ReportPlayerHit(this.playerType);
                }
            }
            
        }
         
    }

    public void Stun(float duration)
    {
        if (state == PlayerState.Stunned) return;

        Debug.Log("���� ��!");
        state = PlayerState.Stunned;
        rb.linearVelocity = Vector3.zero;

        StartCoroutine(PlayerStun(duration));
    }

    IEnumerator PlayerStun(float duration)
    {
        yield return new WaitForSeconds(duration);
        state = PlayerState.Controllable;
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

        if (tile != null)
        {
            currentStoreTile = tile;
            curretnStoreUI.UpdateStoreUI(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        StoreTile tile = other.GetComponent<StoreTile>();

        if (tile != null && tile == currentStoreTile)
        {
            currentStoreTile = null;
            curretnStoreUI.ExitTileUI(this);
        }
    }

    public void ResetStats()
    {
        moveSpeed = PlayerDataManager.Instance.baseMoveSpeed;
        stunTime = PlayerDataManager.Instance.baseStunTime;
    }
}
