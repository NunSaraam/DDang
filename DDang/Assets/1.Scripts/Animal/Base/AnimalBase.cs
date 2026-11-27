using System.Collections;
using System.Collections.Generic;
using Online;
using UnityEngine;

namespace Animal
{
    // 상속하여 OnSpawn만 구현
    // 온라인: 생각과 판정은 방장 쪽에서만. 상대 PC는 OnRemoteSpawn / OnRemoteEvent로 보여 주기만
    public abstract class AnimalBase : MonoBehaviour
    {
        public AnimalData Data { get; private set; }
        public bool IsActive { get; private set; }

        protected GridGenerator Grid { get; private set; }
        protected IReadOnlyList<PlayerMovement> Players => players;

        private PlayerMovement[] players;
        private OnlineAnimal net;

        // 온라인으로 등장한 동물인지 / 방장(또는 로컬) 쪽인지
        protected bool IsOnline => net != null && net.Object != null && net.Object.IsValid;
        protected bool IsHost => !IsOnline || net.HasStateAuthority;

        // 생명주기 AnimalManager에서 호출 (방장·로컬)
        internal void Begin(AnimalData data, GridGenerator grid)
        {
            net = GetComponent<OnlineAnimal>();
            Data = data;
            Grid = grid;
            players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
            IsActive = true;

            OnSpawn();
            StartCoroutine(LifetimeRoutine());
        }

        // 상대 PC: 방장이 등장시킨 동물을 받아 보여 주기만 (AnimalManager가 호출)
        internal void BeginRemote(AnimalData data, GridGenerator grid)
        {
            net = GetComponent<OnlineAnimal>();
            Data = data;
            Grid = grid;
            players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
            IsActive = true;

            OnRemoteSpawn();
        }

        private IEnumerator LifetimeRoutine()
        {
            yield return new WaitForSeconds(Data.duration);
            Finish();
        }

        // 동물이 할 일을 다 끝냈으면 직접 호출해도 됨
        public void Finish()
        {
            if (!IsActive) return;
            if (!IsHost) return;                                        // 상대 PC: 퇴장은 방장이 정함
            IsActive = false;

            StopAllCoroutines();
            OnDespawn();
            if (AnimalManager.HasInstance) AnimalManager.Instance.NotifyFinished(this);
            if (IsOnline) net.Runner.Despawn(net.Object);              // 온라인: 두 PC에서 함께 사라짐
            else Destroy(gameObject);
        }

        // 상대 PC: 방장이 동물을 없앴을 때 정리 (OnlineAnimal이 호출)
        internal void FinishRemote()
        {
            if (!IsActive) return;
            IsActive = false;

            StopAllCoroutines();
            OnDespawn();
            if (AnimalManager.HasInstance) AnimalManager.Instance.NotifyFinished(this);
        }

        // 플레이어 공격에 맞았을 때 (때린 사람 PC에서 호출)
        public void HandleHit(PlayerMovement attacker)
        {
            if (!IsActive) return;
            if (!IsHost)                                                // 상대 PC: 방장에게 알림
            {
                net.RequestHit(attacker.playerType);
                return;
            }
            OnHit(attacker);
        }

        // 방장: 상대가 때렸다는 알림
        internal void HandleHitBy(PlayerType attacker)
        {
            if (IsActive) OnHit(GetPlayer(attacker));
        }

        protected abstract void OnSpawn();                          // 등장 시 1회
        protected virtual void OnDespawn() { }                      // 퇴장 시 정리 (만든 벽, 얼린 타일 해제 등)
        protected virtual void OnHit(PlayerMovement attacker) { }   // 타격 반응 (닭, 황소, 두더지, 말)

        // ── 온라인 연출 ──
        protected virtual void OnRemoteSpawn() { }                      // 상대 PC에서 등장할 때 (보여 주기만)
        protected virtual void OnRemoteEvent(byte id, int a, int b) { } // 상대 PC에서 연출 신호를 받을 때

        // 방장: 상대 PC에 연출 신호 (로컬에서는 아무 일도 안 함)
        protected void SendEvent(byte id, int a = 0, int b = 0)
        {
            if (IsOnline && IsHost) net.SendEvent(id, a, b);
        }

        internal void ReceiveEvent(byte id, int a, int b)
        {
            if (IsActive) OnRemoteEvent(id, a, b);
        }

        protected PlayerMovement GetPlayer(PlayerType type)
        {
            foreach (PlayerMovement p in players)
                if (p != null && p.playerType == type) return p;
            return null;
        }

        protected static PlayerType Opponent(PlayerType type) =>
            type == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;

        // 점수가 높은 플레이어
        protected PlayerMovement GetLeader()
        {
            int p1 = Grid.CountScore(PlayerType.Player1);
            int p2 = Grid.CountScore(PlayerType.Player2);
            PlayerType leader = p1 == p2
                ? (Random.value < 0.5f ? PlayerType.Player1 : PlayerType.Player2)
                : (p1 > p2 ? PlayerType.Player1 : PlayerType.Player2);
            return GetPlayer(leader);
        }

        // ── 결과는 온라인 경로로 ──
        protected void StunPlayer(PlayerMovement player, float duration)
        {
            if (IsOnline) player.GetComponent<OnlineRabbit>().RequestAnimalStun(duration);   // 맞은 토끼의 주인 PC가 기절
            else player.Stun(duration);
        }

        // 방향으로 cells칸 밀기. 장애물, 빈 공간, 맵 끝 앞에서 멈춤
        protected void PushPlayer(PlayerMovement player, Vector3 direction, int cells, float duration = 0.2f)
        {
            if (IsOnline) player.GetComponent<OnlineRabbit>().RequestKnockBack(direction, cells, duration);
            else player.KnockBack(direction, cells, duration);
        }

        protected Vector2Int CurrentCell => Grid.WorldToGridPosition(transform.position);

        // 순간이동. 온라인에서는 상대 화면에서도 미끄러지지 않고 바로 옮겨짐
        protected void TeleportTo(Vector3 position)
        {
            if (IsOnline) net.Teleport(position);
            else transform.position = position;
        }

        protected void PaintCell(Vector2Int cell, PlayerType color)
        {
            if (IsOnline)                                               // 방장이 기록 → 두 화면 반영
            {
                if (OnlineGrid.Instance != null) OnlineGrid.Instance.HostPaint(cell, color);
            }
            else Grid.PaintCell(cell, color);
        }

        protected void PaintArea(Vector2Int center, int radius, PlayerType color)   // radius 1 = 3×3
        {
            for (int x = -radius; x <= radius; x++)
                for (int z = -radius; z <= radius; z++)
                    PaintCell(center + new Vector2Int(x, z), color);
        }

        protected void PaintCross(Vector2Int center, PlayerType color)              // 중심 + 상하좌우 4칸
        {
            PaintCell(center, color);
            PaintCell(center + Vector2Int.up, color);
            PaintCell(center + Vector2Int.down, color);
            PaintCell(center + Vector2Int.left, color);
            PaintCell(center + Vector2Int.right, color);
        }
    }
}
