namespace DDang.Rules
{
    public enum RoundPhase
    {
        None,
        Countdown,      // 시작 전 카운트다운
        Playing,        // 본 라운드
        ExtraTime,      // 추가 시간 15초
        SuddenDeath,    // 먼저 타격한 쪽 승리
        GoldenGoal,     // 결투 라운드 동점 시 먼저 앞서면 승리
        RoundEnd,       // 결과 표시
        Store,
        GameOver,
    }

    public interface IRoundScoreSource
    {
        int PaintableCount { get; }                 // 장애물을 제외 한 칠할 수 있는 칸 수
        int CountTiles(PlayerType player);
    }

    public readonly struct RoundResult
    {
        public readonly PlayerType Winner;
        public readonly int P1Tiles;
        public readonly int P2Tiles;
        public readonly bool WasDuel;

        public RoundResult(PlayerType winner, int p1Tiles, int p2Tiles, bool wasDuel)
        {
            Winner = winner;
            P1Tiles = p1Tiles;
            P2Tiles = p2Tiles;
            WasDuel = wasDuel;
        }
    }
}
