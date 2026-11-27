using Fusion;
using Stats;
using UnityEngine;

namespace Online
{
    // 온라인 칸 주인 목록. 씬에 놓인 NetworkObject라서 주인은 방장
    // 방장: 맵 seed를 정하고, 칠하기 요청을 판정해서 Owners에 적음 → 두 PC 화면에 똑같이 칠해짐
    public class OnlineGrid : NetworkBehaviour
    {
        private const int Side = 32;                                    // 맵 한 변 최대 칸 수 (지금 맵은 15)
        public static OnlineGrid Instance { get; private set; }

        [Networked] public int Seed { get; private set; }               // 0 = 아직 안 정해짐
        [Networked, Capacity(Side * Side)] private NetworkArray<byte> Owners => default;   // 0 = 빈 칸, 1 = P1, 2 = P2

        private GridGenerator grid;
        private ChangeDetector changes;
        private bool applied;                                           // 맵이 만들어진 뒤 처음 한 번 전체 반영했는지

        public override void Spawned()
        {
            Instance = this;
            changes = GetChangeDetector(ChangeDetector.Source.SimulationState);
            if (HasStateAuthority && Seed == 0) Seed = Random.Range(1, int.MaxValue);   // seed는 방장만 정함
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Instance == this) Instance = null;
        }

        // 내 토끼가 부름: 방장에게 칠해 달라고 요청
        public void RequestPaint(Vector2Int c, PlayerType player)
        {
            if (c.x < 0 || c.y < 0 || c.x >= Side || c.y >= Side) return;
            RpcPaint((byte)c.x, (byte)c.y, (byte)player);
        }

        // 누가 보내든 방장 PC에서만 실행됨. 규칙은 BlockPainting과 같음
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcPaint(byte x, byte z, byte player)
        {
            if (!Rules.RoundManager.HasInstance || !Rules.RoundManager.Instance.IsRoundActive) return;   // 라운드 밖 요청은 무시 (카운트다운·라운드 종료 후)
            if (!FindGrid()) return;
            var pT = (PlayerType)player;
            var c = new Vector2Int(x, z);
            if (TryPaint(c, pT) && Random.value < PlayerDataManager.Instance.GetStats(pT).WidePaintChance)
                PaintNeighbor(c, pT);
        }

        // 방장: 난입 동물이 칠함 (굳은 땅 확률 없이 그대로). 장애물·얼린 칸은 건너뜀
        public void HostPaint(Vector2Int c, PlayerType owner)
        {
            if (!HasStateAuthority || !FindGrid() || !grid.IsFloor(c) || grid.IsLocked(c)) return;
            Owners.Set(Index(c), (byte)owner);
        }

        // 상대 칸이면 상대의 굳은 땅 확률만큼 버팀 (주사위는 방장만 굴림)
        private bool TryPaint(Vector2Int c, PlayerType pT)
        {
            if (!grid.IsFloor(c) || grid.IsLocked(c)) return false;
            var owner = (PlayerType)Owners[Index(c)];
            if (owner == pT) return false;

            PlayerType other = pT == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;
            if (owner != PlayerType.None && Random.value < PlayerDataManager.Instance.GetStats(other).HardGroundChance) return false;

            Owners.Set(Index(c), (byte)pT);
            return true;
        }

        // 넓은 발자국: 상하좌우 중 아직 내 색이 아닌 칸 하나
        private void PaintNeighbor(Vector2Int c, PlayerType pT)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            for (int i = 0; i < dirs.Length; i++)
            {
                int j = Random.Range(i, dirs.Length);
                (dirs[i], dirs[j]) = (dirs[j], dirs[i]);
                Vector2Int n = c + dirs[i];
                if (grid.IsFloor(n) && (PlayerType)Owners[Index(n)] != pT)
                {
                    TryPaint(n, pT);
                    return;
                }
            }
        }

        // 양쪽 PC: Owners가 바뀌면 화면의 칸 색을 맞춤
        public override void Render()
        {
            bool changed = false;
            foreach (string name in changes.DetectChanges(this))
                if (name == nameof(Owners)) changed = true;

            if (!FindGrid()) return;
            if (!changed && applied) return;
            applied = true;

            foreach (Vector2Int c in grid.FloorCells)
            {
                var owner = (PlayerType)Owners[Index(c)];
                PlacedObj block = grid.GetBlock(c);
                if (block != null && block.owner != owner) grid.PaintBlock(block, owner);
            }
        }

        private bool FindGrid()
        {
            if (grid == null) grid = FindAnyObjectByType<GridGenerator>();
            return grid != null && grid.FloorCells.Count > 0;           // 맵이 다 만들어졌는지
        }

        private static int Index(Vector2Int c) => c.x * Side + c.y;
    }
}
