using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public int gridSizeX = 10;
    public int gridSizeZ = 10;

    [Header("블럭 세팅")]
    public PlacedObj prefab;
    public Material defaultMat;
    public Material p1Mat;
    public Material p2Mat;

    private int currentGrid;
    public PlacedObj[,] grid;


    void Start()
    {
        Init();
        GenerateGrid();
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
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 pos = new Vector3(x + 0.5f, 0, z + 0.5f);
                PlacedObj obj = Instantiate(prefab, pos, Quaternion.identity, transform);
                obj.SetMaterial(defaultMat, PlayerType.None);
                grid[x, z] = obj;
            }
        }
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
