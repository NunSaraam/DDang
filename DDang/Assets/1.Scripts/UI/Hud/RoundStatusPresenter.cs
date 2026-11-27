using System;
using Rules;

namespace UI
{
    // 상태 문구: 단계가 바뀌면 보여 줄 문구를 정해 View에
    public class RoundStatusPresenter : IDisposable
    {
        private readonly RoundManager round;
        private readonly RoundStatusView view;

        public RoundStatusPresenter(RoundManager round, RoundStatusView view)
        {
            this.round = round;
            this.view = view;

            round.PhaseChanged += OnPhaseChanged;
            OnPhaseChanged(round.Phase);                            // 지금 단계로 한 번 표시
        }

        public void Dispose() => round.PhaseChanged -= OnPhaseChanged;

        private void OnPhaseChanged(RoundPhase phase) => view.SetText(GetStatusText(phase));

        private string GetStatusText(RoundPhase phase) => phase switch
        {
            RoundPhase.ExtraTime => "추가 시간",
            RoundPhase.SuddenDeath => "서든데스! 먼저 때리면 승리",
            RoundPhase.GoldenGoal => "골든골! 먼저 앞서면 승리",
            _ when round.IsDuelRound => "파이널 라운드 · 파이트!",
            _ when round.Match != null && round.Match.IsDeuce => "듀스",
            _ => "",
        };
    }
}
