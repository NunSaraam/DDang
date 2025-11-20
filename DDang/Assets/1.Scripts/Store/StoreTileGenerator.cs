using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StoreTileGenerator : MonoBehaviour
{
    public GameObject shopTilePrefab;

    public Vector2Int randomTilePos = new Vector2Int(3, 3);
    public Vector2Int SpeedTilePos = new Vector2Int(5, 3);
    public Vector2Int stunTilePos = new Vector2Int(7, 3);

    public Sprite randIcon;
    public Sprite speedIcon;
    public Sprite stunIcon;

    private void Start()
    {
        SpawnShopTile(randomTilePos, StoreType.RandomStat, randIcon);
        SpawnShopTile(SpeedTilePos, StoreType.MoveSpeed, speedIcon);
        SpawnShopTile(stunTilePos, StoreType.StunReduce, stunIcon);
    }

    private void SpawnShopTile(Vector2Int pos, StoreType type, Sprite spr)
    {
        Vector3 worldPos = new Vector3(pos.x + .5f, 0.51f, pos.y + .5f);
        GameObject tile = Instantiate(shopTilePrefab, worldPos, Quaternion.identity);
        StoreTile sTile = tile.GetComponent<StoreTile>();
        tile.name = $"{type}Tile";
        sTile.storeType = type;
        sTile.SetImage(spr);
    }
}
