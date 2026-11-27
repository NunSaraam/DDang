using System;
using Store;

namespace UI
{
    // 당근 개수: 그 View 주인의 당근이 바뀌면 View에
    public class CarrotCounterPresenter : IDisposable
    {
        private readonly CoinManager coins;
        private readonly CarrotCounterView view;

        public CarrotCounterPresenter(CoinManager coins, CarrotCounterView view)
        {
            this.coins = coins;
            this.view = view;

            coins.CoinsChanged += OnCoinsChanged;
            view.SetCount(coins.GetCoins(view.Owner), animate: false);   // 처음에는 튀지 않음
        }

        public void Dispose() => coins.CoinsChanged -= OnCoinsChanged;

        private void OnCoinsChanged(PlayerType player, int value)
        {
            if (player == view.Owner) view.SetCount(value, animate: true);
        }
    }
}
