using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GorillaBoss : BossBase
{
    public override void Activate()
    {
        ShuffleTiles();
    }

    public override void Deactivate()
    {

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
    }
}
