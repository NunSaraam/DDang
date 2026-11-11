using System;

namespace Rules
{
    public enum MatchProgress
    {
        Continue,       // 다음 라운드 진행
        Deuce,          // 2 : 2 -> 듀스 돌입
        Duel,           // 듀스 중 3 : 3 -> 결투 라운드
        GameOver,       // 승자 확정
    }

    public class MatchScore
    {
        private readonly int winsToWin;

        public int P1Wins { get; private set; }
        public int P2Wins { get; private set; }
        public bool IsDeuce { get; private set; }
        public bool IsDuel { get; private set; }
        public PlayerType Winner { get; private set; } = PlayerType.None;

        public int RoundNumber => P1Wins + P2Wins + 1;

        public MatchScore(int winsToWin)
        {
            this.winsToWin = winsToWin;
        }

        public MatchProgress RecordRound(PlayerType roundWinner)
        {
            if (roundWinner == PlayerType.None)
                throw new ArgumentException("라운드는 반드시 승자가 있어야 합니다.");
            if (Winner != PlayerType.None)
                return MatchProgress.GameOver;

            if (roundWinner == PlayerType.Player1) P1Wins++;
            else P2Wins++;

            if (IsDuel)                                             // 결투 라운드 승자가 최종 승자
                return Finish(roundWinner);

            if (!IsDeuce)
            {
                if (P1Wins >= winsToWin || P2Wins >= winsToWin)
                    return Finish(roundWinner);

                int deuceAt = winsToWin - 1;                        // 2 : 2
                if (P1Wins == deuceAt && P2Wins == deuceAt)
                {
                    IsDeuce = true;
                    return MatchProgress.Deuce;
                }
                return MatchProgress.Continue;
            }

            int diff = P1Wins - P2Wins;                             // 듀스 : 2연승 시 승리
            if (Math.Abs(diff) >= 2) return Finish(roundWinner);
            if (diff == 0)                                          // 다시 동점(3 : 3) -> 결투
            {
                IsDuel = true;
                return MatchProgress.Duel;
            }
            return MatchProgress.Continue;
        }

        private MatchProgress Finish(PlayerType winner)
        {
            Winner = winner;
            return MatchProgress.GameOver;
        }
    }
}
