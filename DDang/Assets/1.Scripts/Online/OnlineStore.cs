using Fusion;
using Rules;
using Stats;
using Store;
using UnityEngine;

namespace Online
{
    // 온라인 상점 공유. Store 씬에 놓인 NetworkObject라서 주인은 방장
    // 방장: StoreManager가 평소처럼 진행 → 진열 카드·준비·시간을 여기 적고, 당근·증강 변화는 RPC로 알림
    // 상대: 받은 값으로 화면을 그리고, 행동은 방장에게 요청만
    public class OnlineStore : NetworkBehaviour
    {
        private const int MaxOffers = 8;                                // 플레이어당 최대 카드 수 (지금 3)
        public static OnlineStore Instance { get; private set; }

        [Networked, Capacity(MaxOffers * 2)] private NetworkArray<byte> OfferAugment => default;   // 증강 목록 번호
        [Networked, Capacity(MaxOffers * 2)] private NetworkArray<byte> OfferTier => default;
        [Networked, Capacity(MaxOffers * 2)] private NetworkArray<NetworkBool> OfferSold => default;
        [Networked] private NetworkBool OffersWritten { get; set; }     // 방장이 카드를 적었는지
        [Networked] private NetworkBool P1Ready { get; set; }
        [Networked] private NetworkBool P2Ready { get; set; }
        [Networked] private float RemainingTime { get; set; }
        [Networked] public NetworkBool IsOpen { get; private set; }     // 구매·준비를 받는 중인지
        [Networked] public NetworkBool GuestReady { get; private set; } // 상대 PC가 카드를 받았는지

        private StoreManager store;
        private bool follower;
        private float nextReadySend;
        private int lastSignature;

        public override void Spawned()
        {
            Instance = this;
            store = StoreManager.Current;
            follower = !HasStateAuthority;
            if (follower) RoundManager.Instance.FollowPhase(RoundPhase.Store);     // 상대 PC도 상점 단계 (이동 가능)
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            // 상대 PC: 상점을 떠나면 단계를 None으로 → 받은 디버프가 다음 라운드에 적용됨 (PlayerDataManager)
            if (follower && RoundManager.HasInstance) RoundManager.Instance.FollowPhase(RoundPhase.None);
            if (Instance == this) Instance = null;
        }

        public bool IsReady(PlayerType p) => p == PlayerType.Player1 ? P1Ready : P2Ready;

        // 방장: 상점 상태를 매 틱 적음 → 상대 PC로 자동 전달
        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || store == null || !store.HasOffers) return;

            foreach (PlayerType p in new[] { PlayerType.Player1, PlayerType.Player2 })
            {
                StoreOffer[] offers = store.OffersOf(p);
                for (int i = 0; i < offers.Length; i++)
                {
                    int slot = Slot(p, i);
                    OfferAugment.Set(slot, (byte)store.AugmentIndex(offers[i].Augment));
                    OfferTier.Set(slot, (byte)offers[i].Tier);
                    OfferSold.Set(slot, offers[i].Sold);
                }
            }
            OffersWritten = true;
            P1Ready = store.IsReady(PlayerType.Player1);
            P2Ready = store.IsReady(PlayerType.Player2);
            RemainingTime = store.RemainingTime;
            IsOpen = store.IsOpen;
        }

        // 상대 PC: 받은 카드·시간으로 화면 갱신
        public override void Render()
        {
            if (!follower) return;
            if (store == null) store = StoreManager.Current;
            if (store == null || !store.HasOffers || !OffersWritten) return;

            int signature = 17;                                         // 받은 값이 바뀌었는지 확인용
            foreach (PlayerType p in new[] { PlayerType.Player1, PlayerType.Player2 })
            {
                StoreOffer[] offers = store.OffersOf(p);
                for (int i = 0; i < offers.Length; i++)
                {
                    int slot = Slot(p, i);
                    store.ApplyOffer(p, i, OfferAugment[slot], (AugmentTier)OfferTier[slot], OfferSold[slot]);
                    signature = signature * 31 + OfferAugment[slot] * 100 + OfferTier[slot] * 10 + (OfferSold[slot] ? 1 : 0);
                }
            }
            signature = signature * 31 + (P1Ready ? 1 : 0) + (P2Ready ? 2 : 0) + (IsOpen ? 4 : 0);
            if (signature != lastSignature)                             // 준비·열림·카드가 바뀔 때만 화면 갱신 알림
            {
                lastSignature = signature;
                store.NotifyChanged();
            }
            store.ShowTime(RemainingTime);

            // 카드를 받았으면 방장에게 "준비 끝" (놓칠 수 있어서 확인될 때까지 0.5초마다)
            if (!GuestReady && Time.time >= nextReadySend)
            {
                nextReadySend = Time.time + 0.5f;
                RpcGuestReady();
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcGuestReady() => GuestReady = true;

        // ── 상대 PC → 방장: 요청 (방장의 StoreManager가 판정) ──
        public void RequestBuy(PlayerType p, int slot) => RpcBuy((byte)p, (byte)slot);
        public void RequestRefresh(PlayerType p) => RpcRefresh((byte)p);
        public void RequestReady(PlayerType p) => RpcReady((byte)p);
        public void RequestDebuff(PlayerType chooser, DebuffType type) => RpcDebuffChoice((byte)chooser, (byte)type);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcBuy(byte p, byte slot) => store.TryBuy((PlayerType)p, slot);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcRefresh(byte p) => store.TryRefresh((PlayerType)p);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcReady(byte p) => store.ToggleReady((PlayerType)p);

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RpcDebuffChoice(byte chooser, byte type) => store.ChooseDebuff((PlayerType)chooser, (DebuffType)type);

        // ── 방장 → 상대 PC: 결과 적용 (정확한 값을 보내서 두 PC의 당근·증강이 같게 유지) ──
        public void SendBought(PlayerType p, int augment, AugmentTier tier, int price) =>
            RpcBought((byte)p, (byte)augment, (byte)tier, price);
        public void SendSpent(PlayerType p, int amount) => RpcSpent((byte)p, amount);
        public void SendDebuff(PlayerType chooser, DebuffType type) => RpcDebuff((byte)chooser, (byte)type);

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcBought(byte p, byte augment, byte tier, int price)
        {
            if (!follower) return;                                      // 방장은 이미 처리함
            store.ApplyBought((PlayerType)p, augment, (AugmentTier)tier, price);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcSpent(byte p, int amount)
        {
            if (follower) CoinManager.Instance.TrySpend((PlayerType)p, amount);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcDebuff(byte chooser, byte type)
        {
            if (follower) store.ApplyDebuff((PlayerType)chooser, (DebuffType)type);
        }

        private static int Slot(PlayerType p, int i) => (p == PlayerType.Player1 ? 0 : MaxOffers) + i;
    }
}
