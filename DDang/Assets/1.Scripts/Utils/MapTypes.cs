using UnityEngine;

namespace Map
{
    public enum MapTile
    {
        Floor,      // 칠할 수 있는 바닥
        Obstacle,   // 그림자처럼 보이는 장애물 (이동 불가)
        Void,       // 빈 공간 (이동 불가)
    }

    public class MapLayout
    {
        public readonly int Size;
        public readonly int Seed;
        public readonly Vector2Int P1Spawn;
        public readonly Vector2Int P2Spawn;

        private readonly MapTile[,] tiles;

        public MapLayout(int size, int seed, Vector2Int p1Spawn, Vector2Int p2Spawn)
        {
            Size = size;
            Seed = seed;
            P1Spawn = p1Spawn;
            P2Spawn = p2Spawn;
            tiles = new MapTile[size, size];
        }

        public MapTile this[int x, int z]
        {
            get => tiles[x, z];
            set => tiles[x, z] = value;
        }

        public bool InBounds(int x, int z) => x >= 0 && x < Size && z >= 0 && z < Size;

        // 맵 밖도 막힌 것으로 취급
        public bool IsBlocked(int x, int z) => !InBounds(x, z) || tiles[x, z] != MapTile.Floor;

        public Vector2Int Mirror(Vector2Int cell) => new Vector2Int(Size - 1 - cell.x, Size - 1 - cell.y);

        public int CountFloor()
        {
            int count = 0;
            foreach (MapTile tile in tiles)
                if (tile == MapTile.Floor) count++;
            return count;
        }
    }
}
