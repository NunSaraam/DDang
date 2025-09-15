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

        TestPlacedObj obj = Instantiate(prefab, new Vector3(x + 0.5f, 0, z + 0.5f), Quaternion.identity);

        grid[x,z] = obj;
    }


    // 그리드 범위 안쪽인지 확인
    private bool IsInGrid(int x, int z)
    {
        return (x >= 0 && x < gridSize && z >= 0 && z < gridSize);
    }

    // 해당 위치가 비어있는지 확인
    private bool CanInstantiate(int x, int z)
    {
        return grid[x, z] == null;
    }
}
