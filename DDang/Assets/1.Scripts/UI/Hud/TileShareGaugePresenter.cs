using System;
using Rules;

namespace UI
{
    // 점유율 게이지: 칠한 칸 수가 바뀌면 비율을 계산해 View에
    public class TileShareGaugePresenter : IDisposable
    {
        private readonly RoundManager round;
        private readonly TileShareGaugeView view;

        public TileShareGaugePresenter(RoundManager round, TileShareGaugeView view)
        {
            this.round = round;
            this.view = view;

            round.ScoreChanged += OnScoreChanged;
            OnScoreChanged();                                       // 지금 칸 수로 한 번 맞춤
        }

        public void Dispose() => round.ScoreChanged -= OnScoreChanged;

        private void OnScoreChanged()
        {
            int total = round.PaintableCount;
            if (total == 0)
            {
                view.SetTargets(0f, 0f);
                return;
            }
            view.SetTargets((float)round.GetTiles(PlayerType.Player1) / total,
                            (float)round.GetTiles(PlayerType.Player2) / total);
        }
    }
}
