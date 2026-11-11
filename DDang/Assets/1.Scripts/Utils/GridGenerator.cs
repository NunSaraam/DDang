using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;
using Map;
using Cameras;

public class GridGenerator : MonoBehaviour, IRoundScoreSource
{
    public int gridSizeX = 10;
    public int gridSizeZ = 10;

    [Header("블럭 세팅")]
    public PlacedObj prefab;
    public Material defaultMat;
    public Material p1Mat;
    public Material p2Mat;

    [Header("맵 생성 (비어 있으면 기존처럼 gridSize 크기의 평지)")]
    [SerializeField] private MapGenSettings mapSettings;
    [SerializeField] private MapGenSettings duelMapSettings;
    [SerializeField] private GameObject obstaclePrefab;        // 그림자 장애물: 콜라이더 포함, Block 레이어 X, PlacedObj X
    [SerializeField] private GameObject voidPrefab;            // 빈 공간: 보이지 않는 콜라이더

    private int currentGrid;
    public PlacedObj[,] grid;
    public MapLayout Layout { get; private set; }
    private int paintableCount;

    private readonly List<Vector2Int> floorCells = new();
    private readonly HashSet<PlacedObj> lockedBlocks = new();
    private readonly HashSet<Vector2Int> blockedCells = new();     // 뱀 벽처럼 잠깐 막힌 칸
    public IReadOnlyList<Vector2Int> FloorCells => floorCells;

    public int PaintableCount => paintableCount;
    public int CountTiles(PlayerType pT) => CountScore(pT);


    void Start()
    {
        Layout = CreateLayout();
        if (Layout != null) gridSizeX = gridSizeZ = Layout.Size;

        Init();
        GenerateGrid();

        if (Layout != null)
        {
            BuildBoundary(Layout.Size);
            PlacePlayers();
        }

        if (RoundManager.HasInstance)                           // 라운드가 없는 씬(튜토리얼 등)에서는 등록 안 함
            RoundManager.Instance.RegisterScoreSource(this);
    }

    private MapLayout CreateLayout()
    {
        bool isDuel = RoundManager.HasInstance && RoundManager.Instance.IsDuelRound;
        MapGenSettings settings = isDuel ? duelMapSettings : mapSettings;
        if (settings == null) return null;

        int seed = settings.fixedSeed != 0 ? settings.fixedSeed : System.Environment.TickCount;
        Debug.Log($"맵 생성 seed: {seed} ({(isDuel ? "결투" : "일반")})");     // 문제 맵 재현용
        return MapGenerator.Generate(settings, seed);
    }

    private void OnDestroy()
    {
        if (RoundManager.HasInstance) RoundManager.Instance.UnregisterScoreSource(this);
    }


    private void Init()
    {
        grid = new PlacedObj[gridSizeX,gridSizeZ];
    }

    public void PaintBlock(PlacedObj block, PlayerType pT)
    {
        if (block == null || lockedBlocks.Contains(block)) return;     // 얼린 타일은 덮어쓸 수 없음

        PlayerType oldOwner = block.owner;

        Material mat = pT switch                            //switch expression방식 =>는 왼쪽이 입력일 때 오른쪽 값 반환, _는 default:
        {
            PlayerType.Player1 => p1Mat,
            PlayerType.Player2 => p2Mat,
            _ => defaultMat
        };

        block.SetMaterial(mat, pT);                 //땅을 칠하면서, 땅주인도 기록

        if (pT != PlayerType.None && pT != oldOwner)
        {
            AchievementManager.Instance.AddValue(pT, AchievementType.TotalPaintedTiles, 1);
        }
    }

    public void KeyCheckPaintBlock(PlacedObj block, KeyCheckPlayerType kpT)
    {
        if (block == null) return;

        PlayerType oldOwner = block.owner;

        Material mat = kpT switch                            //switch expression방식 =>는 왼쪽이 입력일 때 오른쪽 값 반환, _는 default:
        {
            KeyCheckPlayerType.Player1 => p1Mat,
            KeyCheckPlayerType.Player2 => p2Mat,
            _ => defaultMat
        };

        block.KeyCheckSetMaterial(mat, kpT);                 //땅을 칠하면서, 땅주인도 기록
    }

    public int CountScore(PlayerType pT)
    {
        int score = 0;

        foreach (var block in grid)
        {
            if (block != null && block.owner == pT)
            {
                score++;
            }
        }

        return score;
    }

    private void GenerateGrid()
    {
        paintableCount = 0;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 pos = new Vector3(x + 0.5f, 0, z + 0.5f);
                MapTile tile = Layout != null ? Layout[x, z] : MapTile.Floor;

                switch (tile)
                {
                    case MapTile.Floor:
                        PlacedObj obj = Instantiate(prefab, pos, Quaternion.identity, transform);
                        obj.SetMaterial(defaultMat, PlayerType.None);
                        grid[x, z] = obj;
                        paintableCount++;
                        floorCells.Add(new Vector2Int(x, z));
                        break;
                    case MapTile.Obstacle:
                        if (obstaclePrefab != null) Instantiate(obstaclePrefab, pos, Quaternion.identity, transform);
                        break;
                    case MapTile.Void:
                        if (voidPrefab != null) Instantiate(voidPrefab, pos, Quaternion.identity, transform);
                        break;
                }
            }
        }
    }

    // 맵 바깥 보이지 않는 벽 (5×5 결투 맵에서 떨어지지 않게)
    private void BuildBoundary(int size)
    {
        const float height = 3f, thickness = 1f;
        Transform root = new GameObject("Boundary").transform;
        root.SetParent(transform, false);

        float half = size * 0.5f, y = height * 0.5f;
        CreateWall(root, new Vector3(half, y, -thickness * 0.5f), new Vector3(size + thickness * 2f, height, thickness));
        CreateWall(root, new Vector3(half, y, size + thickness * 0.5f), new Vector3(size + thickness * 2f, height, thickness));
        CreateWall(root, new Vector3(-thickness * 0.5f, y, half), new Vector3(thickness, height, size));
        CreateWall(root, new Vector3(size + thickness * 0.5f, y, half), new Vector3(thickness, height, size));
    }

    private static void CreateWall(Transform parent, Vector3 center, Vector3 size)
    {
        var wall = new GameObject("Wall");
        wall.transform.SetParent(parent, false);
        wall.transform.position = center;
        wall.AddComponent<BoxCollider>().size = size;
    }

    // 플레이어를 스폰 칸으로 옮기고 맵 중앙을 바라보게 함
    private void PlacePlayers()
    {
        Vector3 center = new Vector3(Layout.Size * 0.5f, 0f, Layout.Size * 0.5f);

        foreach (var player in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            Vector2Int cell = player.playerType == PlayerType.Player1 ? Layout.P1Spawn : Layout.P2Spawn;
            Vector3 pos = new Vector3(cell.x + 0.5f, player.transform.position.y, cell.y + 0.5f);
            player.TeleportTo(pos, center);
        }

        foreach (var cam in FindObjectsByType<PlayerDualCamera>(FindObjectsSortMode.None))
            cam.SetTarget(cam.target);                              // 순간이동한 위치로 카메라 즉시 이동
    }


    public Vector2Int WorldToGridPosition(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
    }

    // ── 난입 동물·효과용 칸 도우미 ──
    public bool IsFloor(Vector2Int c) => IsInGrid(c.x, c.y) && grid[c.x, c.y] != null;
    public bool IsWalkable(Vector2Int c) => IsFloor(c) && !blockedCells.Contains(c);

    public void SetBlocked(Vector2Int c, bool blocked)
    {
        if (blocked) blockedCells.Add(c); else blockedCells.Remove(c);
    }
    public PlacedObj GetBlock(Vector2Int c) => IsInGrid(c.x, c.y) ? grid[c.x, c.y] : null;
    public Vector3 CellToWorld(Vector2Int c, float y = 0f) => new Vector3(c.x + 0.5f, y, c.y + 0.5f);
    public Vector2Int RandomFloorCell() => floorCells[Random.Range(0, floorCells.Count)];

    public bool PaintCell(Vector2Int c, PlayerType pT)          // 장애물·얼린 칸이면 false
    {
        PlacedObj block = GetBlock(c);
        if (block == null || lockedBlocks.Contains(block)) return false;
        PaintBlock(block, pT);
        return true;
    }

    public bool IsLocked(Vector2Int c)
    {
        PlacedObj block = GetBlock(c);
        return block != null && lockedBlocks.Contains(block);
    }

    public void SetLocked(Vector2Int c, bool locked)            // 펭귄: 칠한 뒤 잠금
    {
        PlacedObj block = GetBlock(c);
        if (block == null) return;
        if (locked) lockedBlocks.Add(block); else lockedBlocks.Remove(block);
    }

    // from에서 dir로 최대 distance칸. 바닥이 아닌 칸 앞에서 멈춤
    public Vector2Int GetPushDestination(Vector2Int from, Vector2Int dir, int distance)
    {
        Vector2Int cur = IsWalkable(from) ? from : GetNearestFloor(from);
        for (int i = 0; i < distance; i++)
        {
            Vector2Int next = cur + dir;
            if (!IsWalkable(next)) break;
            cur = next;
        }
        return cur;
    }

    // 가장 가까운 바닥 칸 (끼임 탈출용)
    public Vector2Int GetNearestFloor(Vector2Int from)
    {
        var visited = new HashSet<Vector2Int> { from };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(from);
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int c = queue.Dequeue();
            if (IsWalkable(c)) return c;
            foreach (Vector2Int d in dirs)
            {
                Vector2Int n = c + d;
                if (n.x < -1 || n.y < -1 || n.x > gridSizeX || n.y > gridSizeZ) continue;
                if (visited.Add(n)) queue.Enqueue(n);
            }
        }
        return from;
    }


    public void SetObject(int x, int z)                         // 예외처리 디버그 적어놓을것
    {
        if (!IsInGrid(x, z))
        {
            Debug.Log($"{x}, {z}는 그리드 범위 밖입니다.");
            return;
        }
        if (!CanInstantiate(x, z))
        {
            Debug.Log($"{x}, {z}에는 이미 오브젝트가 존재합니다.");
            return;
        }
        if (prefab == null)
        {
            Debug.Log("Prefab이 할당되지 않았습니다.");
            return;
        }

        PlacedObj obj = Instantiate(prefab, new Vector3(x + 0.5f, 0, z + 0.5f), Quaternion.identity);

        grid[x,z] = obj;
    }


    // 그리드 범위 안쪽인지 확인
    private bool IsInGrid(int x, int z)
    {
        return (x >= 0 && x < gridSizeX && z >= 0 && z < gridSizeZ);
    }

    // 해당 위치가 비어있는지 확인
    private bool CanInstantiate(int x, int z)
    {
        return grid[x, z] == null;
    }
}
