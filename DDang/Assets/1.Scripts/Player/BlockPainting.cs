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
    [SerializeField] private float rayDistance = 2f;        // 발밑 블록까지 검사할 거리

    private PlayerMovement player;

    private void Start()
    {
        grid = FindAnyObjectByType<GridGenerator>();
        player = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (player != null && player.state == PlayerState.Stunned) return;    // 기절 중에는 칠하지 않음

        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, mask))
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
