using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyCheckPlayerType
{
    None,
    Player1,
    Player2
}

public class KeyCheckPainting : MonoBehaviour
{
    public KeyCheckPlayerType pT;
    public GridGenerator grid;
    public LayerMask mask;

    private void Start()
    {
        grid = FindObjectOfType<GridGenerator>();
    }

    private void Update()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        if (grid != null && Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2f, mask))   // 거리 2, 레이어 필터
        {
            PlacedObj block = hit.collider.GetComponent<PlacedObj>();
            if (block != null)
            {
                grid.KeyCheckPaintBlock(block, pT);
            }
        }

        Debug.DrawRay(transform.position, Vector3.down * 2f, Color.green);
    }



}
