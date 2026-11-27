using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;
using Stats;
using Online;

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
    [SerializeField] private float rayDistance = 2f;

    private PlayerMovement player;
    private PlacedObj lastBlock;
    private PlayerStats myStats;
    private PlayerStats opponentStats;

    private void Start()
    {
        grid = FindAnyObjectByType<GridGenerator>();
        player = GetComponent<PlayerMovement>();
        myStats = PlayerDataManager.Instance.GetStats(pT);
        opponentStats = PlayerDataManager.Instance.GetStats(pT == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1);
    }

    private void Update()
    {
        if (player != null && player.state == PlayerState.Stunned) return;    // 기절 중에는 칠하지 않음
        if (RoundManager.HasInstance && RoundManager.Instance.Phase == RoundPhase.SuddenDeath) return;  // 서든데스는 타격으로만 승부

        // 온라인: 라운드 진행 중에만 칠함. 그 전에는 "지난 칸"을 비워 두어 시작하는 순간 서 있는 칸부터 같이 칠함
        if (OnlineGrid.Instance != null && !(RoundManager.HasInstance && RoundManager.Instance.IsRoundActive))
        {
            lastBlock = null;
            return;
        }

        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayDistance, mask))
        {
            PlacedObj block = hit.collider.GetComponent<PlacedObj>();
            if (block != null && block != lastBlock)            // 같은 칸에서는 들어설 때 한 번만 시도
            {
                lastBlock = block;
                if (OnlineGrid.Instance != null)                // 온라인: 방장에게 요청만
                    OnlineGrid.Instance.RequestPaint(grid.WorldToGridPosition(block.transform.position), pT);
                else if (TryPaint(block) && Random.value < myStats.WidePaintChance)
                    PaintNeighbor(block);
            }
        }

        Debug.DrawRay(transform.position, Vector3.down * 2f, Color.green);
    }

    // 상대 칸이면 상대의 굳은 땅 확률만큼 버팀. 얼린 칸은 GridGenerator가 막음
    private bool TryPaint(PlacedObj block)
    {
        if (block == null || block.owner == pT) return false;
        if (block.owner != PlayerType.None && Random.value < opponentStats.HardGroundChance) return false;
        grid.PaintBlock(block, pT);
        return block.owner == pT;
    }

    // 넓은 발자국: 상하좌우 중 아직 내 색이 아닌 칸 하나
    private void PaintNeighbor(PlacedObj block)
    {
        Vector2Int c = grid.WorldToGridPosition(block.transform.position);
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        for (int i = 0; i < dirs.Length; i++)
        {
            int j = Random.Range(i, dirs.Length);
            (dirs[i], dirs[j]) = (dirs[j], dirs[i]);
            PlacedObj n = grid.GetBlock(c + dirs[i]);
            if (n != null && n.owner != pT)
            {
                TryPaint(n);
                return;
            }
        }
    }

}
