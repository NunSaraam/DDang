using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreTileManager : MonoBehaviour
{
    public GameObject shopTilePrefab;

    public Vector2Int randomTilePos = new Vector2Int(3, 3);
    public Vector2Int SpeedTilePos = new Vector2Int(5, 3);
    public Vector2Int stunTilePos = new Vector2Int(7, 3);

    public Material randMat;
    public Material speedMat;
    public Material stunMat;

    private void Start()
    {
        SpawnShopTile(randomTilePos, StoreType.RandomStat, randMat);
        SpawnShopTile(SpeedTilePos, StoreType.MoveSpeed, speedMat);
        SpawnShopTile(stunTilePos, StoreType.StunReduce, stunMat);
    }

    private void SpawnShopTile(Vector2Int pos, StoreType type, Material mat)
    {
        Vector3 worldPos = new Vector3(pos.x + .5f, 0, pos.y + .5f);
        GameObject tile = Instantiate(shopTilePrefab, worldPos, Quaternion.identity);
        StoreTile Stile = tile.AddComponent<StoreTile>();
        Stile.storeType = type;
        Stile.SetMaterial(mat);
    }
}
