using System.Collections;
using System.Collections.Generic;
using Stats;
using UnityEngine;

namespace Animal
{
    // 호랑이: 점수가 높은 플레이어를 그 플레이어보다 살짝 빠르게 추격. 닿으면 1.5초 기절시키고,
    //         맵 가운데로 순간이동한 뒤 대상을 다시 고름
    // 대상이 훤히 보이면 직선으로 달려들고, 장애물에 가려지면 대각선을 포함한 최단 경로로 돌아감
    public class Tiger : AnimalBase
    {
        [Header("추격")]
        [SerializeField] private float speedBonus = 0.7f;          // 대상 플레이어 이동속도(증강·디버프 반영) + 이만큼 (칸/초). 0.5~1 정도가 "살짝 빠름"
        [SerializeField] private float turnDegreesPerSecond = 720f; // 실제로 움직이는 방향으로 몸을 돌리는 속도 (도/초)
        [SerializeField] private float catchRadius = 0.6f;         // 이 거리 안에 들어오면 닿은 것으로 판정
        [SerializeField] private float bodyRadius = 0.3f;          // 직선 추격 시 장애물과 이만큼 떨어져야 통과

        [Header("기절")]
        [SerializeField] private float stunDuration = 1.5f;
        [SerializeField] private float minRestAfterCatch = 1.5f;   // 가운데로 순간이동한 뒤 이만큼 쉬고 다시 추격 (0이면 바로)

        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new(1, 1), new(1, -1), new(-1, 1), new(-1, -1),
        };

        private PlayerMovement target;
        private bool caught;

        // 경로 캐시: 출발 칸이나 목표 칸이 바뀔 때만 다시 계산
        private Vector2Int cachedFrom, cachedGoal, cachedNext;
        private bool hasCache;

        protected override void OnSpawn() => StartCoroutine(HuntRoutine());

        private IEnumerator HuntRoutine()
        {
            while (IsActive)
            {
                target = GetLeader();                               // 점수가 높은 플레이어를 대상으로
                caught = false;
                hasCache = false;

                while (IsActive && !caught && target != null)
                {
                    float speed = ChaseSpeed();
                    Vector3 targetPos = target.transform.position;
                    targetPos.y = transform.position.y;

                    if (HasClearPath(transform.position, targetPos))
                    {
                        MoveToward(targetPos, speed);               // 보이면 직선으로
                    }
                    else
                    {
                        Vector2Int from = SafeCell(CurrentCell);
                        Vector2Int goal = SafeCell(Grid.WorldToGridPosition(targetPos));
                        Vector2Int next = NextStep(from, goal);
                        // 다음 칸이 토끼가 있는 칸이거나 이미 같은 칸이면 칸 중앙이 아니라 토끼에게 곧장
                        Vector3 aim = next == goal ? targetPos : Grid.CellToWorld(next, transform.position.y);
                        MoveToward(aim, speed);
                    }

                    CheckCatch();
                    yield return null;
                }

                if (caught)                                         // 잡았으면 가운데로 순간이동 → 다시 1등을 고름
                {
                    Vector2Int center = SafeCell(new Vector2Int(Grid.gridSizeX / 2, Grid.gridSizeZ / 2));
                    TeleportTo(Grid.CellToWorld(center, transform.position.y));
                    hasCache = false;
                    if (minRestAfterCatch > 0f) yield return new WaitForSeconds(minRestAfterCatch);
                }
                else yield return null;
            }
        }

        // 대상 플레이어의 지금 이동속도(증강·디버프·상한 반영) + 보너스
        private float ChaseSpeed() =>
            PlayerDataManager.Instance.GetStats(target.playerType).MoveSpeed + speedBonus;

        // 닿았으면 기절시킴. 이미 기절 중이거나 무적이면 무시
        private void CheckCatch()
        {
            if (target == null || caught) return;

            Vector3 diff = target.transform.position - transform.position;
            diff.y = 0f;
            if (diff.magnitude > catchRadius) return;
            if (target.IsInvincible || target.state == PlayerState.Stunned) return;

            StunPlayer(target, stunDuration);
            caught = true;
        }

        // 이동한 방향으로 몸을 빠르게 돌림 (목표가 아니라 실제 움직임 기준 → 옆걸음 없음)
        private void MoveToward(Vector3 goal, float speed)
        {
            Vector3 before = transform.position;
            transform.position = Vector3.MoveTowards(before, goal, speed * Time.deltaTime);

            Vector3 moved = transform.position - before;
            moved.y = 0f;
            if (moved.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(moved), turnDegreesPerSecond * Time.deltaTime);
        }

        // 몸통 폭(bodyRadius)을 고려해서 직선 경로에 막힌 칸이 없는지 확인
        private bool HasClearPath(Vector3 from, Vector3 to)
        {
            Vector3 dir = to - from;
            dir.y = 0f;
            float length = dir.magnitude;
            if (length < 0.01f) return true;

            Vector3 side = Vector3.Cross(Vector3.up, dir / length) * bodyRadius;
            const float step = 0.2f;

            for (float d = 0f; d <= length; d += step)
            {
                Vector3 p = from + dir * (d / length);
                if (!Grid.IsWalkable(Grid.WorldToGridPosition(p))) return false;
                if (!Grid.IsWalkable(Grid.WorldToGridPosition(p + side))) return false;
                if (!Grid.IsWalkable(Grid.WorldToGridPosition(p - side))) return false;
            }
            return true;
        }

        // 대각선을 포함한 최단 경로의 첫 칸 (다익스트라). 대각선은 양옆 칸이 모두 비어 있을 때만
        private Vector2Int NextStep(Vector2Int from, Vector2Int goal)
        {
            if (hasCache && from == cachedFrom && goal == cachedGoal) return cachedNext;

            var cost = new Dictionary<Vector2Int, float> { [from] = 0f };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int> { [from] = from };
            var open = new List<Vector2Int> { from };
            var closed = new HashSet<Vector2Int>();

            while (open.Count > 0)
            {
                int best = 0;
                for (int i = 1; i < open.Count; i++)
                    if (cost[open[i]] < cost[open[best]]) best = i;
                Vector2Int c = open[best];
                open.RemoveAt(best);
                if (!closed.Add(c)) continue;
                if (c == goal) break;

                foreach (Vector2Int d in Directions)
                {
                    Vector2Int n = c + d;
                    if (closed.Contains(n) || !Grid.IsWalkable(n)) continue;

                    bool diagonal = d.x != 0 && d.y != 0;
                    if (diagonal && (!Grid.IsWalkable(c + new Vector2Int(d.x, 0)) ||
                                     !Grid.IsWalkable(c + new Vector2Int(0, d.y))))
                        continue;                                   // 장애물 모서리를 가로지르지 않게

                    float newCost = cost[c] + (diagonal ? 1.414f : 1f);
                    if (cost.TryGetValue(n, out float old) && old <= newCost) continue;

                    cost[n] = newCost;
                    cameFrom[n] = c;
                    open.Add(n);
                }
            }

            Vector2Int next = from;
            if (cameFrom.ContainsKey(goal) && goal != from)
            {
                next = goal;
                while (cameFrom[next] != from) next = cameFrom[next];
            }
            else if (goal == from)
            {
                next = goal;
            }

            cachedFrom = from;
            cachedGoal = goal;
            cachedNext = next;
            hasCache = true;
            return next;
        }

        private Vector2Int SafeCell(Vector2Int c) => Grid.IsWalkable(c) ? c : Grid.GetNearestFloor(c);
    }
}
