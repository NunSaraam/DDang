using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrid : MonoBehaviour
{
    public int gridSizeX = 10;
    public int gridSizeZ = 10;

    [Header("�� ����")]
    public TestPlacedObj prefab;
    public Material defaultMat;
    public Material p1Mat;
    public Material p2Mat;


    private int currentGrid;
    private TestPlacedObj[,] grid;


    void Start()
    {
        Init();
        GenerateGrid();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            float x = Random.value * gridSizeX;
            float z = Random.value * gridSizeZ;

            Vector3 temp = new Vector3(x,0,z);
            Vector2Int pos = WorldToGridPosition(temp);
            SetObject(pos.x, pos.y);
        }
    }

    private void Init()
    {
        grid = new TestPlacedObj[gridSizeX,gridSizeZ];
    }

    public void PaintBlock(TestPlacedObj block, PlayerType pT)
    {
        if (block == null) return;

        Material mat = pT switch
        {
            PlayerType.Player1 => p1Mat,
            PlayerType.Player2 => p2Mat,
            _ => defaultMat
        };

        block.SetMaterial(mat);
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 pos = new Vector3(x + 0.5f, 0, z + 0.5f);
                TestPlacedObj obj = Instantiate(prefab, pos, Quaternion.identity, transform);
                obj.SetMaterial(defaultMat);
                grid[x, z] = obj;
            }
        }
    }


    public Vector2Int WorldToGridPosition(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
    }


    public void SetObject(int x, int z)                         // ����ó�� ����� ���������
    {
        if (!IsInGrid(x, z))
        {
            Debug.Log($"{x}, {z}�� �׸��� ���� ���Դϴ�.");
            return;
        }
        if (!CanInstantiate(x, z))
        {
            Debug.Log($"{x}, {z}���� �̹� ������Ʈ�� �����մϴ�.");
            return;
        }
        if (prefab == null)
        {
            Debug.Log("Prefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        TestPlacedObj obj = Instantiate(prefab, new Vector3(x + 0.5f, 0, z + 0.5f), Quaternion.identity);

        grid[x,z] = obj;
    }


    // �׸��� ���� �������� Ȯ��
    private bool IsInGrid(int x, int z)
    {
        return (x >= 0 && x < gridSizeX && z >= 0 && z < gridSizeZ);
    }

    // �ش� ��ġ�� ����ִ��� Ȯ��
    private bool CanInstantiate(int x, int z)
    {
        return grid[x, z] == null;
    }
}
