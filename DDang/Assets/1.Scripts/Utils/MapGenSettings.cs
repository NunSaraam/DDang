using UnityEngine;

namespace Map
{
    [CreateAssetMenu(fileName = "MapGenSettings", menuName = "DDang/Map Gen Settings")]
    public class MapGenSettings : ScriptableObject
    {
        [Header("크기")]
        [Min(3)] public int size = 15;

        [Header("스폰 (칸 좌표 x, z) - P2는 점대칭 위치")]
        public Vector2Int p1Spawn = new Vector2Int(7, 1);

        [Header("장애물")]
        [Range(0f, 0.5f)] public float minObstacleRatio = 0.10f;
        [Range(0f, 0.5f)] public float maxObstacleRatio = 0.15f;
        [Range(0f, 1f)] public float voidChance = 0.25f;        // 빈 공간일 확률

        [Header("장애물 금지 구역")]
        public int spawnSafeRadius = 2;                         // 스폰 주변
        public int centerSafeRadius = 1;                        // 1 = 중앙 3×3

        [Header("생성 시도")]
        public int maxAttempts = 20;
        public int stampTriesPerAttempt = 200;
        [Tooltip("0이면 매번 랜덤 시드")] public int fixedSeed = 0;
    }
}
