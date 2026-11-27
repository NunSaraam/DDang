using System;
using Rules;

namespace UI
{
    // 머리 위 말풍선: 칠한 칸 수·단계·기절 상태 → 말풍선
    // 상점 단계면 안내 문구 칸만, 그 외에는 칸 수 (기절 중이면 기절 그림)
    public class PlayerBubblePresenter : IDisposable
    {
        private readonly RoundManager round;
        private readonly PlayerMovement player;
        private readonly PlayerBubbleView view;
        private int shownScore = -1;

        public PlayerBubblePresenter(RoundManager round, PlayerMovement player, PlayerBubbleView view)
        {
            this.round = round;
            this.player = player;
            this.view = view;

            round.ScoreChanged += OnScoreChanged;
            round.PhaseChanged += OnPhaseChanged;
            player.StateChanged += OnStateChanged;

            OnPhaseChanged(round.Phase);                            // 지금 상태로 한 번 맞춤
            OnStateChanged(player.state);
            shownScore = round.GetTiles(player.playerType);
            view.ShowScore(shownScore, animate: false);
        }

        public void Dispose()
        {
            round.ScoreChanged -= OnScoreChanged;
            round.PhaseChanged -= OnPhaseChanged;
            player.StateChanged -= OnStateChanged;
        }

        private void OnScoreChanged()
        {
            int score = round.GetTiles(player.playerType);
            if (score == shownScore) return;                        // 상대 칸만 바뀌었으면 그대로
            bool grew = score > shownScore;
            shownScore = score;
            view.ShowScore(score, animate: grew);                   // 늘어날 때만 톡
        }

        private void OnPhaseChanged(RoundPhase phase) =>
            view.SetMode(phase == RoundPhase.Store ? PlayerBubbleView.Mode.Store : PlayerBubbleView.Mode.Game);

        private void OnStateChanged(PlayerState state) => view.ShowStun(state == PlayerState.Stunned);
    }
}
