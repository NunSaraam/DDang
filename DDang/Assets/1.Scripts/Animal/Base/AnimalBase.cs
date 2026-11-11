using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animal
{
    // 상속하여 OnSpawn만 구현
    public abstract class AnimalBase : MonoBehaviour
    {
        public AnimalData Data { get; private set; }
        public bool IsActive { get; private set; }

        protected GridGenerator Grid { get; private set; }
        protected IReadOnlyList<PlayerMovement> Players => players;

        private PlayerMovement[] players;

        // 생명주기 AnimalManager에서 호출
        internal void Begin(AnimalData data, GridGenerator grid)
        {
            Data = data;
            Grid = grid;
            players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
            IsActive = true;

            OnSpawn();
            StartCoroutine(LifetimeRoutine());
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
            IsActive = false;

            StopAllCoroutines();
            OnDespawn();
            if (AnimalManager.HasInstance) AnimalManager.Instance.NotifyFinished(this);
            Destroy(gameObject);
        }

        // 플레이어 공격에 맞았을 때
        public void HandleHit(PlayerMovement attacker)
        {
            if (IsActive) OnHit(attacker);
        }

        protected abstract void OnSpawn();                          // 등장 시 1회
        protected virtual void OnDespawn() { }                      // 퇴장 시 정리 (만든 벽, 얼린 타일 해제 등)
        protected virtual void OnHit(PlayerMovement attacker) { }   // 타격 반응 (닭, 황소, 두더지, 말)

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

        protected void StunPlayer(PlayerMovement player, float duration) => player.Stun(duration);

        // 방향으로 cells칸 밀기. 장애물, 빈 공간, 맵 끝 앞에서 멈춤
        protected void PushPlayer(PlayerMovement player, Vector3 direction, int cells, float duration = 0.2f) =>
            player.KnockBack(direction, cells, duration);

        protected Vector2Int CurrentCell => Grid.WorldToGridPosition(transform.position);

        protected void PaintCell(Vector2Int cell, PlayerType color) => Grid.PaintCell(cell, color);

        protected void PaintArea(Vector2Int center, int radius, PlayerType color)   // radius 1 = 3×3
        {
            for (int x = -radius; x <= radius; x++)
                for (int z = -radius; z <= radius; z++)
                    Grid.PaintCell(center + new Vector2Int(x, z), color);
        }

        protected void PaintCross(Vector2Int center, PlayerType color)              // 중심 + 상하좌우 4칸
        {
            Grid.PaintCell(center, color);
            Grid.PaintCell(center + Vector2Int.up, color);
            Grid.PaintCell(center + Vector2Int.down, color);
            Grid.PaintCell(center + Vector2Int.left, color);
            Grid.PaintCell(center + Vector2Int.right, color);
        }
    }
}
