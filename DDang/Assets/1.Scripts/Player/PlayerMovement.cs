using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Rules;


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

    [Header("전투")]
    [SerializeField] private float attackRange = 1f;            // 전방 1칸
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float invincibleTime = 0.5f;       // 무적 시간
    [SerializeField] private float blinkInterval = 0.1f;        // 무적 중 깜빡임 간격

    private Renderer[] renderers;
    private float invincibleUntil;

    public bool IsInvincible => Time.time < invincibleUntil;

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
        renderers = GetComponentsInChildren<Renderer>();


        if (playerType == PlayerType.Player1)
        {
            moveSpeed = PlayerDataManager.Instance.ClampMoveSpeed(PlayerDataManager.Instance.p1MoveSpeed);
            stunTime = PlayerDataManager.Instance.ClampStunTime(PlayerDataManager.Instance.p1StunTime);

        }
        else if (playerType == PlayerType.Player2)
        {
            moveSpeed = PlayerDataManager.Instance.ClampMoveSpeed(PlayerDataManager.Instance.p2MoveSpeed);
            stunTime = PlayerDataManager.Instance.ClampStunTime(PlayerDataManager.Instance.p2StunTime);
        }

        if (RoundManager.Instance.IsDuelRound)                     // 결투 라운드는 기본 능력치
        {
            moveSpeed = PlayerDataManager.Instance.baseMoveSpeed;
            stunTime = PlayerDataManager.Instance.baseStunTime;
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

            if (state == PlayerState.Controllable && RoundManager.Instance.IsRoundActive
                && !RoundManager.Instance.IsDuelRound)               // 결투 라운드는 공격 금지
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
        // 몸 앞 0 ~ attackRange 구간을 덮는 구
        float radius = attackRange * 0.5f;
        Vector3 checkPosition = playerTransform.position + playerTransform.forward * radius;

        Collider[] hitColliders = Physics.OverlapSphere(checkPosition, radius, mask);

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
                    if (target.IsInvincible || target.state == PlayerState.Stunned) continue;

                    target.Stun(target.stunTime);                  // 맞는 쪽의 기절 시간 적용

                    QuestManager.Instance?.OnStunLanded(this.playerType);
                    RoundManager.Instance.ReportPlayerHit(this.playerType);
                }
            }
            
        }
         
    }

    public void Stun(float duration)
    {
        if (state == PlayerState.Stunned || IsInvincible) return;

        state = PlayerState.Stunned;
        rb.linearVelocity = Vector3.zero;

        StartCoroutine(PlayerStun(duration));
    }

    IEnumerator PlayerStun(float duration)
    {
        yield return new WaitForSeconds(duration);
        state = PlayerState.Controllable;

        invincibleUntil = Time.time + invincibleTime;
        StartCoroutine(BlinkWhileInvincible());
    }

    IEnumerator BlinkWhileInvincible()
    {
        bool visible = true;
        while (IsInvincible)
        {
            visible = !visible;
            SetRenderersVisible(visible);
            yield return new WaitForSeconds(blinkInterval);
        }
        SetRenderersVisible(true);
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (Renderer r in renderers) r.enabled = visible;
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

    public void TeleportTo(Vector3 position, Vector3 lookAt)
    {
        Rigidbody body = GetComponent<Rigidbody>();                 // Start 전에 불릴 수 있음
        Vector3 dir = lookAt - position;
        dir.y = 0f;
        Quaternion rotation = dir.sqrMagnitude > 0.01f ? Quaternion.LookRotation(dir) : transform.rotation;

        transform.SetPositionAndRotation(position, rotation);
        body.position = position;
        body.rotation = rotation;
        body.linearVelocity = Vector3.zero;
    }

    public void ResetStats()
    {
        moveSpeed = PlayerDataManager.Instance.baseMoveSpeed;
        stunTime = PlayerDataManager.Instance.baseStunTime;
    }
}
