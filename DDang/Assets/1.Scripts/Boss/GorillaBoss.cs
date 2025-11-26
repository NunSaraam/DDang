using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GorillaBoss : BossBase
{
    public float shuffleInterval = 1f;
    public int shuffleCount = 3;

    public override void Activate()
    {
        StartCoroutine(ShuffleRoutine());
    }

    public override void Deactivate()
    {

    }

    IEnumerator ShuffleRoutine()
    {
        for (int i = 0; i < shuffleCount; i++)
        {
            ShuffleTiles();

            yield return new WaitForSeconds(shuffleInterval);
        }
    }
    void ShuffleTiles()
    {
        GridGenerator grid = FindObjectOfType<GridGenerator>();
        List<PlayerType> owner = new List<PlayerType>();


        for (int x = 0; x < grid.gridSizeX; x++)
        {
            for (int z = 0; z < grid.gridSizeZ; z++)
            {
                owner.Add(grid.grid[x, z].onwer);
            }
        }

        for (int i = owner.Count - 1; i > 0; i--)
        {
            int shuffle = Random.Range(0, i + 1);
            (owner[i], owner[shuffle]) = (owner[shuffle], owner[i]);
        }

        int index = 0;

        for (int x = 0; x < grid.gridSizeX; x++)
        {
            for (int z = 0; z < grid.gridSizeZ; z++)
            {
                PlayerType newowner = owner[index++];
                grid.PaintBlock(grid.grid[x, z], newowner);
            }
        }

        Destroy(gameObject, 3f);
    }

}
