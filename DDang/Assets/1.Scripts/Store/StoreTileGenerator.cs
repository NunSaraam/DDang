using UnityEngine;

namespace Store
{
    // P1 구역에 증강·새로고침·레디 타일. P2 구역은 맵 중심 기준 점대칭 위치
    public class StoreTileGenerator : MonoBehaviour
    {
        public GameObject shopTilePrefab;
        [SerializeField] private int gridSize = 15;

        [Header("P1 구역 (칸 좌표 x, z)")]
        [SerializeField] private Vector2Int augmentPos = new(7, 3);
        [SerializeField] private Vector2Int refreshPos = new(3, 3);
        [SerializeField] private Vector2Int readyPos = new(11, 3);

        [Header("아이콘")]
        [SerializeField] private Sprite augmentIcon;
        [SerializeField] private Sprite refreshIcon;
        [SerializeField] private Sprite readyIcon;

        private void Start()
        {
            SpawnZone(PlayerType.Player1, mirror: false);
            SpawnZone(PlayerType.Player2, mirror: true);
        }

        private void SpawnZone(PlayerType owner, bool mirror)
        {
            Spawn(owner, StoreTileType.Augment, augmentPos, mirror, augmentIcon);
            Spawn(owner, StoreTileType.Refresh, refreshPos, mirror, refreshIcon);
            Spawn(owner, StoreTileType.Ready, readyPos, mirror, readyIcon);
        }

        private void Spawn(PlayerType owner, StoreTileType type, Vector2Int cell, bool mirror, Sprite icon)
        {
            if (mirror) cell = new Vector2Int(gridSize - 1 - cell.x, gridSize - 1 - cell.y);
            Vector3 pos = new Vector3(cell.x + 0.5f, 0.51f, cell.y + 0.5f);
            GameObject tile = Instantiate(shopTilePrefab, pos, Quaternion.identity);
            tile.GetComponent<StoreTile>().Setup(owner, type, icon);
        }
    }
}
