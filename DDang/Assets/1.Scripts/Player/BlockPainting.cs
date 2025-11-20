using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType
{
    None,
    Player1,
    Player2
}

public class BlockPainting : MonoBehaviour
{
    public PlayerType pT;
    public GridGenerator grid;
    public LayerMask mask;

    private void Start()
    {
        grid = FindObjectOfType<GridGenerator>();    
    }

    private void Update()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, mask))
        {
            PlacedObj block = hit.collider.GetComponent<PlacedObj>();
            if (block != null)
            {
                grid.PaintBlock(block, pT);
            }
        }

        Debug.DrawRay(transform.position, Vector3.down * 2f, Color.green);
    }

}
