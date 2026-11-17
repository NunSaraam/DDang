using System.Collections;
using System.Collections.Generic;
using Rules;
using Stats;
using UnityEngine;

namespace Store
{
    public enum StoreTileType 
    { 
        Augment,
        Refresh,
        Ready
    }

    public class StoreOffer
    {
        public AugmentTier Tier;            // 상점 입장·새로고침 때 추첨
        public AugmentData Augment;         // 상점 입장·새로고침 때 추첨
        public bool Sold;
    }

    // 상점 진행: 카드 추첨, 구매, 새로고침, 패자 디버프, 레디, 상점 시간
    public class StoreManager : MonoBehaviour
    {
        public static StoreManager Current { get; private set; }

        [SerializeField] private StoreSettings settings;
        [SerializeField] private StoreUI storeUI;
        [SerializeField] private AugmentCardUI p1Cards;
        [SerializeField] private AugmentCardUI p2Cards;

        public StoreSettings Settings => settings;

        private const int DebuffCount = 3;

        private readonly Dictionary<PlayerType, StoreOffer[]> offers = new();
        private readonly HashSet<PlayerType> readyPlayers = new();
        private PlayerType debuffChooser = PlayerType.None;     // 아직 디버프를 고르지 않은 패자
        private bool closing;

        private void Awake() => Current = this;

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        private void Start()
        {
            offers[PlayerType.Player1] = RollOffers();
            offers[PlayerType.Player2] = RollOffers();

            RoundResult? last = RoundManager.Instance.LastResult;
            if (last.HasValue && last.Value.Winner != PlayerType.None)
            {
                debuffChooser = Opponent(last.Value.Winner);
                CardsOf(debuffChooser).OpenDebuffs();
            }

            StartCoroutine(StoreRoutine());
        }

        private IEnumerator StoreRoutine()
        {
            for (float t = settings.storeTime; t > 0f && readyPlayers.Count < 2; t -= Time.deltaTime)
            {
                storeUI.ShopingTime(t);
                yield return null;
            }

            closing = true;                                         // 이후 구매·레디 입력 무시
            if (readyPlayers.Count == 2)                            // 둘 다 레디 -> 3초 카운트다운
            {
                for (float t = settings.readyCountdown; t > 0f; t -= Time.deltaTime)
                {
                    storeUI.ShopingTime(t);
                    yield return null;
                }
            }

            if (debuffChooser != PlayerType.None)                   // 시간 안에 못 고르면 랜덤
                ChooseDebuff(debuffChooser, (DebuffType)Random.Range(0, DebuffCount));

            p1Cards.Close();
            p2Cards.Close();
            RoundManager.Instance.FinishStore();
        }

        // 타일 위에서 공격키
        public void Interact(PlayerMovement player, StoreTile tile)
        {
            PlayerType p = player.playerType;
            if (closing || tile.Owner != p) return;                // 자기 구역 타일만
            if (IsReady(p) && tile.Type != StoreTileType.Ready) return;    // 레디 중에는 레디 해제만

            switch (tile.Type)
            {
                case StoreTileType.Augment: CardsOf(p).OpenAugments(offers[p]); break;
                case StoreTileType.Refresh: TryRefresh(p); break;
                case StoreTileType.Ready: ToggleReady(p); break;
            }
            storeUI.UpdateStoreUI(player);
        }

        public int GetPrice(PlayerType p) =>
            settings.basePrice + PlayerDataManager.Instance.GetLoadout(p).PurchaseCount * settings.priceStepPerPurchase;

        public bool TryBuy(PlayerType p, int slot)
        {
            StoreOffer offer = offers[p][slot];
            if (closing || offer.Sold) return false;
            if (!CoinManager.Instance.TrySpend(p, GetPrice(p))) return false;

            PlayerDataManager.Instance.AddAugment(p, offer.Augment, offer.Tier);
            offer.Sold = true;
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.AddValue(p, AchievementType.TotalStoreUsed, 1);
            return true;
        }

        public void ChooseDebuff(PlayerType chooser, DebuffType type)
        {
            if (chooser != debuffChooser) return;
            PlayerDataManager.Instance.GiveDebuff(Opponent(chooser), type);
            debuffChooser = PlayerType.None;
        }

        public bool IsReady(PlayerType p) => readyPlayers.Contains(p);

        private void TryRefresh(PlayerType p)
        {
            if (!CoinManager.Instance.TrySpend(p, settings.refreshCost)) return;
            foreach (StoreOffer o in offers[p])                     // 새로고침하면 등급과 증강을 모두 다시 뽑음
            {
                o.Tier = settings.RollTier();
                o.Augment = PickAugment();
                o.Sold = false;
            }
        }

        private void ToggleReady(PlayerType p)
        {
            if (!readyPlayers.Remove(p)) readyPlayers.Add(p);
        }

        private StoreOffer[] RollOffers()
        {
            var result = new StoreOffer[settings.cardsPerOffer];
            for (int i = 0; i < result.Length; i++)
                result[i] = new StoreOffer { Tier = settings.RollTier(), Augment = PickAugment() };
            return result;
        }

        private AugmentData PickAugment() =>
            settings.augments[Random.Range(0, settings.augments.Count)];

        private AugmentCardUI CardsOf(PlayerType p) => p == PlayerType.Player1 ? p1Cards : p2Cards;

        private static PlayerType Opponent(PlayerType p) =>
            p == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;
    }
}
