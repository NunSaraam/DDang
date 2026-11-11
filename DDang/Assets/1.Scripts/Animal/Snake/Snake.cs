using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animal
{
    // 뱀: 바닥 칸을 따라 기어다니고, 지나간 칸은 뱀이 퇴장할 때까지 벽이 됨
    public class Snake : AnimalBase
    {
        [Header("이동")]
        [SerializeField] private float cellsPerSecond = 4f;
        [SerializeField, Range(0f, 1f)] private float straightChance = 0.7f;   // 직진을 유지할 확률
        [SerializeField, Range(0f, 1f)] private float centerBias = 0.5f;       // 꺾을 때 중앙 쪽을 고를 확률
        [SerializeField] private float burrowAfter = 1f;                       // 이 시간 이상 갇히면 다른 칸으로 이동

        [Header("벽")]
        [SerializeField] private GameObject wallPrefab;
        [SerializeField, Min(0)] private int maxWalls = 0;          // 0 = 무제한, 넘으면 가장 오래된 벽부터 제거
        [SerializeField] private float playerClearance = 1f;        // 칸 중심에서 이 거리 안에 플레이어가 있으면 벽 생략

        private static readonly Vector2Int[] Directions =
            { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        private readonly Dictionary<Vector2Int, GameObject> walls = new();
        private readonly Queue<Vector2Int> wallOrder = new();
        private Vector2Int direction;

        protected override void OnSpawn()
        {
            if (wallPrefab == null || wallPrefab.GetComponentInChildren<Collider>(true) == null)
                Debug.LogWarning("Snake: Wall Prefab에 콜라이더가 없습니다.", this);

            direction = Directions[Random.Range(0, Directions.Length)];
            StartCoroutine(CrawlRoutine());
        }

        protected override void OnDespawn()
        {
            // 퇴장 시 벽 전부 제거
            foreach (Vector2Int cell in new List<Vector2Int>(walls.Keys)) RemoveWall(cell);
            wallOrder.Clear();
        }

        private IEnumerator CrawlRoutine()
        {
            Vector2Int cell = CurrentCell;
            float stuckTime = 0f;

            while (IsActive)
            {
                if (!TryPickNext(cell, out Vector2Int next))
                {
                    stuckTime += Time.deltaTime;
                    if (stuckTime >= burrowAfter)                   // 자기 벽에 갇힘 -> 다른 칸으로
                    {
                        stuckTime = 0f;
                        cell = Burrow();
                    }
                    yield return null;
                    continue;
                }

                stuckTime = 0f;
                direction = next - cell;
                yield return MoveTo(next);
                TryPlaceWall(cell);                                 // 떠나온 칸을 벽으로
                cell = next;
            }
        }

        private bool TryPickNext(Vector2Int cell, out Vector2Int next)
        {
            bool huggingEdge = IsEdge(cell) && IsEdge(cell + direction);    // 가장자리를 따라가는 중이면 직진 안 함
            if (!huggingEdge && Random.value < straightChance && Grid.IsWalkable(cell + direction))
            {
                next = cell + direction;
                return true;
            }

            var options = new List<Vector2Int>();
            foreach (Vector2Int d in Directions)
                if (d != -direction && Grid.IsWalkable(cell + d)) options.Add(cell + d);   // 뒤로는 돌지 않음

            if (options.Count == 0 && Grid.IsWalkable(cell - direction))
                options.Add(cell - direction);                        // 막다른 길이면 되돌아감

            if (options.Count == 0)
            {
                next = cell;
                return false;
            }

            next = Random.value < centerBias ? ClosestToCenter(options) : options[Random.Range(0, options.Count)];
            return true;
        }

        private Vector2Int Burrow()
        {
            Vector2Int target = Grid.RandomFloorCell();
            for (int i = 0; i < 20 && !Grid.IsWalkable(target); i++) target = Grid.RandomFloorCell();

            transform.position = Grid.CellToWorld(target, transform.position.y);
            return target;
        }

        private bool IsEdge(Vector2Int c) =>
            c.x <= 0 || c.y <= 0 || c.x >= Grid.gridSizeX - 1 || c.y >= Grid.gridSizeZ - 1;

        private Vector2Int ClosestToCenter(List<Vector2Int> options)
        {
            var center = new Vector2(Grid.gridSizeX * 0.5f, Grid.gridSizeZ * 0.5f);
            Vector2Int best = options[0];
            foreach (Vector2Int c in options)
                if (Vector2.Distance(c, center) < Vector2.Distance(best, center)) best = c;
            return best;
        }

        private IEnumerator MoveTo(Vector2Int cell)
        {
            Vector3 start = transform.position;
            Vector3 end = Grid.CellToWorld(cell, start.y);
            transform.rotation = Quaternion.LookRotation(end - start);

            float duration = 1f / cellsPerSecond;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(start, end, t / duration);
                yield return null;
            }
            transform.position = end;
        }

        private void TryPlaceWall(Vector2Int cell)
        {
            if (walls.ContainsKey(cell) || IsPlayerNear(cell)) return;    // 플레이어가 끼이지 않게

            GameObject wall = Instantiate(wallPrefab, Grid.CellToWorld(cell), Quaternion.identity);
            walls[cell] = wall;
            wallOrder.Enqueue(cell);
            Grid.SetBlocked(cell, true);

            while (maxWalls > 0 && walls.Count > maxWalls)
                RemoveWall(wallOrder.Dequeue());
        }

        private void RemoveWall(Vector2Int cell)
        {
            if (!walls.Remove(cell, out GameObject wall)) return;
            Grid.SetBlocked(cell, false);
            if (wall != null) Destroy(wall);
        }

        private bool IsPlayerNear(Vector2Int cell)
        {
            Vector3 center = Grid.CellToWorld(cell);
            foreach (PlayerMovement p in Players)
            {
                if (p == null) continue;
                Vector3 d = p.transform.position - center;
                if (Mathf.Abs(d.x) < playerClearance && Mathf.Abs(d.z) < playerClearance) return true;
            }
            return false;
        }
    }
}
