using System;
using Rules;

namespace UI
{
    // RoundManager(모델)의 이벤트를 듣고 라운드 HUD에 보여 줄 내용을 정함
    public class RoundHudPresenter : IDisposable
    {
        private readonly RoundManager round;
        private readonly RoundHudView view;

        public RoundHudPresenter(RoundManager round, RoundHudView view)
        {
            this.round = round;
            this.view = view;

            round.RoundStarted += OnRoundStarted;
            round.PhaseChanged += OnPhaseChanged;
            round.RemainingTimeChanged += OnRemainingTimeChanged;
            round.MatchProgressed += OnMatchProgressed;

            view.ClearTimes();
            OnPhaseChanged(round.Phase);                            // 지금 상태로 한 번 맞춤
        }

        public void Dispose()
        {
            round.RoundStarted -= OnRoundStarted;
            round.PhaseChanged -= OnPhaseChanged;
            round.RemainingTimeChanged -= OnRemainingTimeChanged;
            round.MatchProgressed -= OnMatchProgressed;
        }

        private void OnRoundStarted(int roundNumber) => view.ShowWait(true);

        private void OnPhaseChanged(RoundPhase phase)
        {
            if (phase == RoundPhase.Playing) view.ShowWait(false);  // 본 라운드가 시작되면 카운트다운 창 닫기
            RefreshWins();                                          // 새 매치(0:0)도 여기서 반영
        }

        private void OnRemainingTimeChanged(float time)
        {
            if (round.Phase == RoundPhase.Countdown) view.SetWaitTime(time);
            else if (round.IsRoundActive) view.SetRoundTime(time);
        }

        private void OnMatchProgressed(MatchProgress progress) => RefreshWins();

        private void RefreshWins()
        {
            MatchScore match = round.Match;
            view.SetWins(match?.P1Wins ?? 0, match?.P2Wins ?? 0);
        }
    }
}
