using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrid : MonoBehaviour
{
    public TestPlacedObj prefab;
    public int gridSize = 10;

    private TestPlacedObj[,] grid;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            float x = Random.value * gridSize;
            float z = Random.value * gridSize;

            Vector3 temp = new Vector3(x,0,z);
            Vector2Int pos = WorldToGridPosition(temp);
            SetObject(pos.x, pos.y);
        }
    }

    public Vector2Int WorldToGridPosition(Vector3 pos)
    {
        return new Vector2Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.z));
    }

    private void Init()
    {
        grid = new TestPlacedObj[gridSize, gridSize];
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
        return (x >= 0 && x < gridSize && z >= 0 && z < gridSize);
    }

    // �ش� ��ġ�� ����ִ��� Ȯ��
    private bool CanInstantiate(int x, int z)
    {
        return grid[x, z] == null;
    }
}
