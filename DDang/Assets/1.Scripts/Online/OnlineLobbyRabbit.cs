using System.Collections;
using System.Linq;
using Cameras;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Online
{
    // 대기실 토끼(튜토리얼 토끼). 내 토끼만 조종하고, 상대 토끼는 받은 위치만 보여 줌
    // 칠하기(KeyCheckPainting)는 받은 위치대로 각 PC에서 칠하므로 따로 동기화하지 않아도 두 화면이 같음
    // 2P 자리 토끼는 상대가 들어와야 등장
    [RequireComponent(typeof(TutorialMovement))]
    public class OnlineLobbyRabbit : NetworkBehaviour
    {
        [SerializeField] private PlayerType player;                     // 이 토끼가 몇 P 자리인지
        private bool? mine;
        private bool? present;
        private float nextRequest;
        private bool moveToStart;                                       // 주인 권한을 받으면 시작 위치로

        private Vector3 startPosition;
        private Quaternion startRotation;
        private Renderer[] renderers;
        private Collider[] colliders;
        private KeyCheckPainting painting;

        private void Awake()
        {
            startPosition = transform.position;                         // 씬에 놓인 시작 위치 기억
            startRotation = transform.rotation;
            renderers = GetComponentsInChildren<Renderer>();
            colliders = GetComponentsInChildren<Collider>();
            painting = GetComponent<KeyCheckPainting>();
        }

        public override void Render()
        {
            // 2P 자리는 상대가 들어와야 등장 (1P 자리는 항상 누군가 있음)
            bool isPresent = player == PlayerType.Player1 || Runner.ActivePlayers.Count() >= 2;
            if (present != isPresent)
            {
                present = isPresent;
                SetPresent(isPresent);
            }

            bool isMine = OnlineSession.Instance.CurrentSlot() == player;    // 그때그때 계산 → 첫 프레임부터 정확

            // 내 토끼인데 주인이 아니면 다시 요청 (상대가 잠깐 가져갔어도 회복)
            if (isMine && !HasStateAuthority && Time.time >= nextRequest)
            {
                nextRequest = Time.time + 0.5f;
                Object.RequestStateAuthority();
            }

            var body = GetComponent<Rigidbody>();

            // 권한을 받은 뒤에 시작 위치로 순간이동 (받기 전에는 상대에게 전달되지 않음)
            if (moveToStart && HasStateAuthority)
            {
                moveToStart = false;
                GetComponent<OnlineTransformSync>().Teleport(startPosition, startRotation);
                body.position = startPosition;
                body.linearVelocity = Vector3.zero;
            }

            // 내 번호가 바뀔 때마다(상대가 나가서 2P → 1P 등) 조종 대상을 다시 정함
            if (mine == isMine) return;
            mine = isMine;

            var input = GetComponent<PlayerInput>();
            input.enabled = isMine;
            if (isMine && player == PlayerType.Player2) input.SwitchCurrentActionMap("PlayerActions");   // 온라인은 WASD + Space
            GetComponent<TutorialMovement>().enabled = isMine;
            body.isKinematic = !isMine;
            body.interpolation = isMine ? RigidbodyInterpolation.Interpolate : RigidbodyInterpolation.None;   // 상대 토끼는 받은 위치로만 움직이므로 끔

            if (!isMine) return;
            moveToStart = true;                                         // 내 토끼가 되면 시작 위치에서 출발

            if (Camera.main != null && Camera.main.TryGetComponent(out PlayerDualCamera cam))
                cam.SetTarget(transform);                               // 카메라가 내 토끼를 따라감
        }

        // 숨김/등장: 모습·충돌·칠하기를 함께 켜고 끔
        private void SetPresent(bool on)
        {
            foreach (Renderer r in renderers) r.enabled = on;
            foreach (Collider c in colliders) c.enabled = on;
            if (painting != null) painting.enabled = on;
            if (on) StartCoroutine(PopIn());
        }

        // 등장 연출: 모델이 0 → 1 크기로 커짐 (각 PC에서 따로 재생)
        private IEnumerator PopIn()
        {
            Animator animator = GetComponent<TutorialMovement>().animator;
            Transform model = animator != null ? animator.transform : transform;
            for (float t = 0f; t < 0.25f; t += Time.deltaTime)
            {
                model.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, t / 0.25f);
                yield return null;
            }
            model.localScale = Vector3.one;
        }

        public void PlayAttackForOthers() => RpcAttack();

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void RpcAttack()
        {
            Animator animator = GetComponent<TutorialMovement>().animator;
            if (animator != null) animator.SetTrigger("Attack");
        }
    }
}
