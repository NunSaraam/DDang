using Fusion;
using Rules;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Online
{
    // 씬에 놓인 토끼(Player1/Player2)에 붙임. 내 번호의 토끼만 내가 조종하고, 상대 토끼는 받은 위치만 보여 줌
    // 오프라인에서는 Spawned가 불리지 않아 아무 일도 안 함 → 로컬 2인 모드 그대로
    [RequireComponent(typeof(PlayerMovement))]
    public class OnlineRabbit : NetworkBehaviour
    {
        public bool IsMine { get; private set; }

        [Networked] private PlayerState State { get; set; }             // 기절 말풍선 등 상대 화면 표시용
        [Networked] private NetworkBool Invincible { get; set; }        // 무적 깜빡임 표시용

        private PlayerMovement movement;
        private Renderer[] renderers;

        public override void Spawned()
        {
            movement = GetComponent<PlayerMovement>();
            renderers = GetComponentsInChildren<Renderer>();
            IsMine = movement.playerType == OnlineSession.Instance.MyPlayer;

            if (IsMine)
            {
                if (!HasStateAuthority) Object.RequestStateAuthority();     // 2P는 방장에게서 주인 권한을 넘겨받음
                if (movement.playerType == PlayerType.Player2)
                    GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerActions");   // 온라인은 둘 다 WASD + Space
                return;
            }

            // 상대 토끼: 내 키보드로 안 움직이고, 물리 대신 받은 위치로만 움직임
            GetComponent<PlayerInput>().enabled = false;
            movement.enabled = false;
            if (TryGetComponent(out BlockPainting painting)) painting.enabled = false;   // 칠하기는 방장이 판정
            GetComponent<Rigidbody>().isKinematic = true;
        }

        // 내 토끼: 기절·무적 상태를 매 틱 적음 → 상대 PC로 전달
        public override void FixedUpdateNetwork()
        {
            if (!IsMine || !HasStateAuthority) return;
            State = movement.state;
            Invincible = movement.IsInvincible;
        }

        // 상대 토끼: 받은 상태를 화면에 반영
        public override void Render()
        {
            if (IsMine) return;
            movement.SetRemoteState(State);
            movement.SetRemoteInvincible(Invincible);                   // 방장 PC의 동물이 무적인 토끼를 알 수 있게
            bool visible = !Invincible || Mathf.Repeat(Time.time, 0.2f) < 0.1f;   // 무적 중 깜빡임
            foreach (Renderer r in renderers) r.enabled = visible;
        }

        // ── 공격 ──
        // 때린 쪽 PC가 부름: 맞은 토끼의 주인 PC에게 기절 판정을 맡김
        public void RequestStun(float stunBonus, PlayerType attacker) => RpcStun(stunBonus, (byte)attacker);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcStun(float stunBonus, byte attacker)
        {
            if (movement.state == PlayerState.Stunned || movement.IsInvincible) return;   // 주인 PC 기준으로 판정
            movement.Stun(movement.stunTime + stunBonus);
            RpcStunLanded(attacker);
        }

        // 진짜 기절이 걸렸을 때만 두 PC에 알림 → 퀘스트·서든데스가 같은 순서로 처리됨
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcStunLanded(byte attacker)
        {
            var pT = (PlayerType)attacker;
            QuestManager.Instance?.OnStunLanded(pT);
            if (RoundManager.HasInstance && !RoundManager.Instance.IsFollower)
                RoundManager.Instance.ReportPlayerHit(pT);                   // 서든데스 판정은 방장만
        }

        // ── 난입 동물 (방장 PC) → 맞은 토끼의 주인 PC. 퀘스트·서든데스와는 상관없음 ──
        public void RequestAnimalStun(float duration) => RpcAnimalStun(duration);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcAnimalStun(float duration) => movement.Stun(duration);

        public void RequestKnockBack(Vector3 direction, int cells, float duration) => RpcKnockBack(direction, cells, duration);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcKnockBack(Vector3 direction, int cells, float duration) => movement.KnockBack(direction, cells, duration);

        // 내 공격 모션을 상대 화면에서도 재생
        public void PlayAttackForOthers() => RpcAttack();

        [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
        private void RpcAttack() => movement.animator.SetTrigger("Attack");
    }
}
