using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Animal
{
    // 호랑이: 점수가 높은 플레이어를 추격. 닿으면 1.5초 기절시키고, 잠깐 쉰 뒤 대상을 다시 고름
    // 대상이 훤히 보이면 직선으로 달려들고, 장애물에 가려지면 대각선을 포함한 최단 경로로 돌아감
    public class Tiger : AnimalBase
    {
        [Header("추격")]
        [SerializeField] private float cellsPerSecond = 4.5f;      // 이동 속도 (플레이어 기본 5칸/초)
        [SerializeField] private float catchRadius = 0.6f;         // 이 거리 안에 들어오면 닿은 것으로 판정
        [SerializeField] private float bodyRadius = 0.3f;          // 직선 추격 시 장애물과 이만큼 떨어져야 통과
        [SerializeField] private float turnSpeed = 12f;            // 몸을 돌리는 속도

        [Header("기절")]
        [SerializeField] private float stunDuration = 1.5f;
        [FormerlySerializedAs("restAfterCatch")]
        [SerializeField] private float escapeTime = 1.5f;          // 잡힌 플레이어가 깨어난 뒤 도망칠 시간. 호랑이는 기절 시간 + 이 시간만큼 쉼

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
                    Vector3 targetPos = target.transform.position;
                    targetPos.y = transform.position.y;

                    if (HasClearPath(transform.position, targetPos))
                    {
                        MoveToward(targetPos);                      // 보이면 직선으로
                    }
                    else
                    {
                        Vector2Int from = SafeCell(CurrentCell);
                        Vector2Int goal = SafeCell(Grid.WorldToGridPosition(targetPos));
                        Vector2Int next = NextStep(from, goal);
                        MoveToward(Grid.CellToWorld(next, transform.position.y));
                    }

                    CheckCatch();
                    yield return null;
                }

                if (caught) yield return new WaitForSeconds(stunDuration + escapeTime);   // 기절 + 도망칠 시간
                else yield return null;
            }
        }

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

        private void MoveToward(Vector3 goal)
        {
            Vector3 dir = goal - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion look = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
            }
            transform.position = Vector3.MoveTowards(transform.position, goal, cellsPerSecond * Time.deltaTime);
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