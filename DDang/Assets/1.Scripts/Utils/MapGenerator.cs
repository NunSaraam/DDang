using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    // 점대칭 스탬프 방식 맵 생성기. 같은 시드면 같은 맵이 나온다.
    public static class MapGenerator
    {
        private readonly struct Stamp
        {
            public readonly MapTile Tile;
            public readonly Vector2Int[] Cells;

            public Stamp(MapTile tile, params Vector2Int[] cells)
            {
                Tile = tile;
                Cells = cells;
            }
        }

        private static readonly Stamp[] ObstacleStamps =
        {
            new Stamp(MapTile.Obstacle, new Vector2Int(0, 0)),                                              // 바위 1×1
            new Stamp(MapTile.Obstacle, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)),  // 울타리 1×3
            new Stamp(MapTile.Obstacle, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1)),  // 덤불 L자
        };

        private static readonly Stamp PondStamp =
            new Stamp(MapTile.Void, new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1));  // 연못 2×2

        private static readonly Vector2Int[] Directions =
            { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        public static MapLayout Generate(MapGenSettings settings, int seed)
        {
            for (int attempt = 0; attempt < settings.maxAttempts; attempt++)
            {
                MapLayout layout = TryGenerate(settings, new System.Random(seed + attempt), seed);
                if (layout != null) return layout;
            }

            Debug.LogWarning($"맵 생성 실패 (seed {seed}) → 장애물 없는 맵 사용");
            return CreateEmpty(settings, seed);
        }

        public static MapLayout CreateEmpty(MapGenSettings settings, int seed)
        {
            Vector2Int p1 = settings.p1Spawn;
            Vector2Int p2 = new Vector2Int(settings.size - 1 - p1.x, settings.size - 1 - p1.y);
            return new MapLayout(settings.size, seed, p1, p2);
        }

        private static MapLayout TryGenerate(MapGenSettings settings, System.Random rng, int seed)
        {
            MapLayout layout = CreateEmpty(settings, seed);

            int total = settings.size * settings.size;
            int minBlocked = Mathf.CeilToInt(total * settings.minObstacleRatio);
            int maxBlocked = Mathf.FloorToInt(total * settings.maxObstacleRatio);
            if (maxBlocked == 0) return layout;                     // 장애물 없는 설정 (결투 맵)

            int target = rng.Next(minBlocked, maxBlocked + 1);
            int blocked = 0;

            for (int i = 0; i < settings.stampTriesPerAttempt && blocked < target; i++)
            {
                Stamp stamp = rng.NextDouble() < settings.voidChance
                    ? PondStamp
                    : ObstacleStamps[rng.Next(ObstacleStamps.Length)];
                var origin = new Vector2Int(rng.Next(settings.size), rng.Next(settings.size));

                List<Vector2Int> cells = GetMirroredCells(layout, stamp, rng.Next(4), origin);
                if (cells == null || blocked + cells.Count > maxBlocked) continue;
                if (!CanPlace(layout, settings, cells)) continue;

                foreach (Vector2Int c in cells) layout[c.x, c.y] = stamp.Tile;

                if (HasNarrowCell(layout))                          // 폭 1칸 통로가 생기면 되돌림
                {
                    foreach (Vector2Int c in cells) layout[c.x, c.y] = MapTile.Floor;
                    continue;
                }
                blocked += cells.Count;
            }

            if (blocked < minBlocked) return null;
            if (!IsAllFloorConnected(layout)) return null;
            return layout;
        }

        // 스탬프 칸 + 점대칭 칸 (중복 제거). 맵 밖으로 나가면 null
        private static List<Vector2Int> GetMirroredCells(MapLayout layout, Stamp stamp, int quarterTurns, Vector2Int origin)
        {
            var cells = new List<Vector2Int>();
            foreach (Vector2Int offset in stamp.Cells)
            {
                Vector2Int c = origin + Rotate(offset, quarterTurns);
                if (!layout.InBounds(c.x, c.y)) return null;

                AddUnique(cells, c);
                AddUnique(cells, layout.Mirror(c));
            }
            return cells;
        }

        private static bool CanPlace(MapLayout layout, MapGenSettings settings, List<Vector2Int> cells)
        {
            var center = new Vector2Int(settings.size / 2, settings.size / 2);
            foreach (Vector2Int c in cells)
            {
                if (layout[c.x, c.y] != MapTile.Floor) return false;                          // 겹침
                if (Chebyshev(c, layout.P1Spawn) <= settings.spawnSafeRadius) return false;
                if (Chebyshev(c, layout.P2Spawn) <= settings.spawnSafeRadius) return false;
                if (Chebyshev(c, center) <= settings.centerSafeRadius) return false;
            }
            return true;
        }

        // 양옆 또는 위아래가 모두 막힌 바닥 = 폭 1칸 통로
        private static bool HasNarrowCell(MapLayout layout)
        {
            for (int x = 0; x < layout.Size; x++)
            {
                for (int z = 0; z < layout.Size; z++)
                {
                    if (layout[x, z] != MapTile.Floor) continue;

                    bool pinchedX = layout.IsBlocked(x - 1, z) && layout.IsBlocked(x + 1, z);
                    bool pinchedZ = layout.IsBlocked(x, z - 1) && layout.IsBlocked(x, z + 1);
                    if (pinchedX || pinchedZ) return true;
                }
            }
            return false;
        }

        // P1 스폰에서 플러드필로 모든 바닥에 도달할 수 있는지
        private static bool IsAllFloorConnected(MapLayout layout)
        {
            var visited = new bool[layout.Size, layout.Size];
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(layout.P1Spawn);
            visited[layout.P1Spawn.x, layout.P1Spawn.y] = true;
            int reached = 0;

            while (queue.Count > 0)
            {
                Vector2Int c = queue.Dequeue();
                reached++;

                foreach (Vector2Int dir in Directions)
                {
                    Vector2Int n = c + dir;
                    if (layout.IsBlocked(n.x, n.y) || visited[n.x, n.y]) continue;
                    visited[n.x, n.y] = true;
                    queue.Enqueue(n);
                }
            }
            return reached == layout.CountFloor();
        }

        private static Vector2Int Rotate(Vector2Int v, int quarterTurns) => quarterTurns switch
        {
            1 => new Vector2Int(-v.y, v.x),
            2 => new Vector2Int(-v.x, -v.y),
            3 => new Vector2Int(v.y, -v.x),
            _ => v,
        };

        private static int Chebyshev(Vector2Int a, Vector2Int b) => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

        private static void AddUnique(List<Vector2Int> list, Vector2Int cell)
        {
            if (!list.Contains(cell)) list.Add(cell);
        }
    }
}
