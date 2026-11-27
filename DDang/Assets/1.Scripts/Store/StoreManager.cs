using System;
using System.Collections;
using System.Collections.Generic;
using Online;
using Rules;
using Stats;
using UnityEngine;
using Random = UnityEngine.Random;

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
    // 온라인: 방장이 진행하고 OnlineStore로 공유. 상대 PC는 받은 값으로 보여 주고 행동은 요청만
    public class StoreManager : MonoBehaviour
    {
        public static StoreManager Current { get; private set; }

        [SerializeField] private StoreSettings settings;

        public StoreSettings Settings => settings;

        // 화면은 직접 부르지 않고 이벤트로 알림 → UI.StoreUiInstaller가 만든 Presenter들이 반영
        public event Action<float> TimeChanged;                     // 남은 상점 시간
        public event Action StateChanged;                           // 준비·열림·진열 카드·당근이 바뀜 (안내 문구·카드 갱신)
        public event Action<PlayerType> AugmentCardsRequested;      // 증강 타일에서 공격키 → 그 사람 카드 창 열기
        public event Action<PlayerType> DebuffChoiceRequested;      // 진 사람에게 디버프 선택 창
        public event Action Closing;                                // 상점 끝 → 카드 창 닫기

        public void NotifyChanged() => StateChanged?.Invoke();     // 온라인 상대 PC: OnlineStore가 받은 값이 바뀌면 호출

        private const int DebuffCount = 3;

        private readonly Dictionary<PlayerType, StoreOffer[]> offers = new();
        private readonly HashSet<PlayerType> readyPlayers = new();
        private PlayerType debuffChooser = PlayerType.None;     // 아직 디버프를 고르지 않은 패자
        private bool opened;                                    // 상점 시간이 시작됐는지 (온라인: 상대 PC 준비 후)
        private bool closing;

        public float RemainingTime { get; private set; }

        // 온라인에서 방장이 아닌 PC: 직접 진행하지 않고 OnlineStore가 받은 대로 따라감
        private static bool IsFollower => RoundManager.Instance.IsFollower;

        // 구매·준비를 받는 중인지
        public bool IsOpen => IsFollower
            ? OnlineStore.Instance != null && OnlineStore.Instance.IsOpen
            : opened && !closing;

        public bool HasOffers => offers.Count == 2;

        private void Awake() => Current = this;

        private void OnDestroy()
        {
            if (Current == this) Current = null;
        }

        private void Start()
        {
            RoundResult? last = RoundManager.Instance.LastResult;
            if (last.HasValue && last.Value.Winner != PlayerType.None)
            {
                debuffChooser = Opponent(last.Value.Winner);
                if (IsMine(debuffChooser)) DebuffChoiceRequested?.Invoke(debuffChooser);    // 온라인: 진 사람 화면에만
            }

            if (IsFollower)                                         // 진열 카드·시간은 OnlineStore가 채움
            {
                offers[PlayerType.Player1] = EmptyOffers();
                offers[PlayerType.Player2] = EmptyOffers();
                return;
            }

            offers[PlayerType.Player1] = RollOffers();
            offers[PlayerType.Player2] = RollOffers();
            StartCoroutine(StoreRoutine());
        }

        private IEnumerator StoreRoutine()
        {
            if (OnlineSession.IsOnline)                             // 온라인: 상대 PC가 카드를 받을 때까지 대기
                yield return new WaitUntil(() => OnlineStore.Instance != null && OnlineStore.Instance.GuestReady);
            opened = true;
            NotifyChanged();

            for (float t = settings.storeTime; t > 0f && readyPlayers.Count < 2; t -= Time.deltaTime)
            {
                ShowTime(t);
                yield return null;
            }

            closing = true;                                         // 이후 구매·레디 입력 무시
            NotifyChanged();
            if (readyPlayers.Count == 2)                            // 둘 다 레디 -> 3초 카운트다운
            {
                for (float t = settings.readyCountdown; t > 0f; t -= Time.deltaTime)
                {
                    ShowTime(t);
                    yield return null;
                }
            }

            if (debuffChooser != PlayerType.None)                   // 시간 안에 못 고르면 랜덤
                ChooseDebuff(debuffChooser, (DebuffType)Random.Range(0, DebuffCount));

            Closing?.Invoke();                                      // 카드 창 닫기
            RoundManager.Instance.FinishStore();
        }

        public void ShowTime(float t)
        {
            RemainingTime = t;
            TimeChanged?.Invoke(t);
        }

        // 타일 위에서 공격키
        public void Interact(PlayerMovement player, StoreTile tile)
        {
            PlayerType p = player.playerType;
            if (!IsOpen || tile.Owner != p) return;                 // 자기 구역 타일만
            if (IsReady(p) && tile.Type != StoreTileType.Ready) return;    // 레디 중에는 레디 해제만

            switch (tile.Type)
            {
                case StoreTileType.Augment: AugmentCardsRequested?.Invoke(p); break;
                case StoreTileType.Refresh:
                    if (IsFollower) OnlineStore.Instance.RequestRefresh(p); else TryRefresh(p);
                    break;
                case StoreTileType.Ready:
                    if (IsFollower) OnlineStore.Instance.RequestReady(p); else ToggleReady(p);
                    break;
            }
        }

        public int GetPrice(PlayerType p) =>
            settings.basePrice + PlayerDataManager.Instance.GetLoadout(p).PurchaseCount * settings.priceStepPerPurchase;

        public bool TryBuy(PlayerType p, int slot)
        {
            StoreOffer offer = offers[p][slot];
            if (!IsOpen || offer.Sold) return false;
            int price = GetPrice(p);
            if (CoinManager.Instance.GetCoins(p) < price) return false;

            if (IsFollower)                                         // 온라인 상대 PC: 방장에게 요청 (결과는 OnlineStore가 적용)
            {
                OnlineStore.Instance.RequestBuy(p, slot);
                return true;
            }

            CoinManager.Instance.TrySpend(p, price);
            PlayerDataManager.Instance.AddAugment(p, offer.Augment, offer.Tier);
            offer.Sold = true;
            if (OnlineSession.IsOnline) OnlineStore.Instance.SendBought(p, AugmentIndex(offer.Augment), offer.Tier, price);
            NotifyChanged();
            return true;
        }

        public void ChooseDebuff(PlayerType chooser, DebuffType type)
        {
            if (chooser != debuffChooser) return;
            if (IsFollower)
            {
                OnlineStore.Instance.RequestDebuff(chooser, type);  // 적용은 방장이 알려 줄 때
                return;
            }
            ApplyDebuff(chooser, type);
            if (OnlineSession.IsOnline) OnlineStore.Instance.SendDebuff(chooser, type);
        }

        public bool IsReady(PlayerType p) => IsFollower
            ? OnlineStore.Instance != null && OnlineStore.Instance.IsReady(p)
            : readyPlayers.Contains(p);

        // 방장(로컬): 직접 또는 상대 요청(OnlineStore)으로 호출
        public void TryRefresh(PlayerType p)
        {
            if (!IsOpen || IsReady(p)) return;
            if (!CoinManager.Instance.TrySpend(p, settings.refreshCost)) return;
            foreach (StoreOffer o in offers[p])                     // 새로고침하면 등급과 증강을 모두 다시 뽑음
            {
                o.Tier = settings.RollTier();
                o.Augment = PickAugment();
                o.Sold = false;
            }
            if (OnlineSession.IsOnline) OnlineStore.Instance.SendSpent(p, settings.refreshCost);
            NotifyChanged();
        }

        public void ToggleReady(PlayerType p)
        {
            if (!IsOpen) return;
            if (!readyPlayers.Remove(p)) readyPlayers.Add(p);
            NotifyChanged();
        }

        // ── 온라인 상대 PC: OnlineStore가 받은 값을 적용 ──
        public StoreOffer[] OffersOf(PlayerType p) => offers[p];
        public int AugmentIndex(AugmentData augment) => settings.augments.IndexOf(augment);

        public void ApplyOffer(PlayerType p, int slot, int augment, AugmentTier tier, bool sold)
        {
            StoreOffer o = offers[p][slot];
            o.Augment = settings.augments[augment];
            o.Tier = tier;
            o.Sold = sold;
        }

        public void ApplyBought(PlayerType p, int augment, AugmentTier tier, int price)
        {
            CoinManager.Instance.TrySpend(p, price);
            PlayerDataManager.Instance.AddAugment(p, settings.augments[augment], tier);
            NotifyChanged();
        }

        public void ApplyDebuff(PlayerType chooser, DebuffType type)
        {
            PlayerDataManager.Instance.GiveDebuff(Opponent(chooser), type);
            debuffChooser = PlayerType.None;
            NotifyChanged();
        }

        private StoreOffer[] RollOffers()
        {
            var result = new StoreOffer[settings.cardsPerOffer];
            for (int i = 0; i < result.Length; i++)
                result[i] = new StoreOffer { Tier = settings.RollTier(), Augment = PickAugment() };
            return result;
        }

        private StoreOffer[] EmptyOffers()
        {
            var result = new StoreOffer[settings.cardsPerOffer];
            for (int i = 0; i < result.Length; i++) result[i] = new StoreOffer();
            return result;
        }

        private AugmentData PickAugment() =>
            settings.augments[Random.Range(0, settings.augments.Count)];

        // 로컬은 둘 다 내 것, 온라인은 내 번호만
        private static bool IsMine(PlayerType p) => !OnlineSession.IsOnline || OnlineSession.Instance.MyPlayer == p;

        private static PlayerType Opponent(PlayerType p) =>
            p == PlayerType.Player1 ? PlayerType.Player2 : PlayerType.Player1;
    }
}
