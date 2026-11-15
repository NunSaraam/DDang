using System;
using Rules;
using Stats;
using UnityEngine;

namespace Store
{
    // 당근(한 판용 재화). 라운드가 끝나면 보상 지급
    public class CoinManager : SingletonMonoBehaviour<CoinManager>
    {
        [SerializeField] private StoreSettings settings;

        public event Action<PlayerType, int> CoinsChanged;

        private int p1Coins;
        private int p2Coins;
        private RoundManager round;

        private void Start()
        {
            if (Instance != this) return;
            round = RoundManager.Instance;
            if (round != null) round.RoundEnded += OnRoundEnded;
        }

        protected override void OnDestroy()
        {
            if (round != null) round.RoundEnded -= OnRoundEnded;
            base.OnDestroy();
        }

        public int GetCoins(PlayerType player) => player == PlayerType.Player1 ? p1Coins : p2Coins;

        public void AddCoins(PlayerType player, int amount) => SetCoins(player, GetCoins(player) + amount);

        public bool TrySpend(PlayerType player, int amount)
        {
            if (GetCoins(player) < amount) return false;
            SetCoins(player, GetCoins(player) - amount);
            return true;
        }

        public void ResetCoins()
        {
            SetCoins(PlayerType.Player1, 0);
            SetCoins(PlayerType.Player2, 0);
        }

        private void SetCoins(PlayerType player, int value)
        {
            if (player == PlayerType.Player1) p1Coins = value;
            else p2Coins = value;
            CoinsChanged?.Invoke(player, value);
        }

        private void OnRoundEnded(RoundResult result)
        {
            if (result.WasDuel || result.Winner == PlayerType.None) return;     // 파이널 라운드 뒤에는 상점 없음
            GiveReward(PlayerType.Player1, result.P1Tiles, result.Winner);
            GiveReward(PlayerType.Player2, result.P2Tiles, result.Winner);
        }

        // 승자 8 / 패자 10 + 칠한 칸 × (10% + 수확) + 당근 보너스
        private void GiveReward(PlayerType player, int tiles, PlayerType winner)
        {
            PlayerStats s = PlayerDataManager.Instance.GetStats(player);
            int baseReward = player == winner ? settings.winnerReward : settings.loserReward;
            int tileReward = Mathf.RoundToInt(tiles * (settings.tileRewardRatio + s.HarvestRatio));
            AddCoins(player, baseReward + tileReward + s.CarrotBonus);
        }
    }
}
