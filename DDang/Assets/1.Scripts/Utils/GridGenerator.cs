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
        if (block == null) return;

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
