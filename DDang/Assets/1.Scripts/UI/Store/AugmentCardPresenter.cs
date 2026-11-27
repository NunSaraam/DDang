using System;
using Online;
using Stats;
using Store;
using UnityEngine;

namespace UI
{
    // 카드 창: 열기·닫기, 좌우 선택, 공격키 = 구매/결정, 아래 = 닫기 (열려 있는 동안 토끼 이동 입력을 받음)
    public class AugmentCardPresenter : IPlayerInputReceiver, IDisposable
    {
        private enum Mode { Closed, Augment, Debuff }

        private static readonly (DebuffType type, string name, string effect)[] Debuffs =
        {
            (DebuffType.MoveSpeedDown, "이속 감소", "상대 이동속도 ÷{0}"),
            (DebuffType.StunTimeUp, "기절 시간 증가", "상대 기절 시간 ×{0}"),
            (DebuffType.AttackCooldownUp, "공격 쿨타임 증가", "상대 공격 쿨타임 ×{0}"),
        };

        private readonly StoreManager store;
        private readonly PlayerMovement player;                     // 이 창 주인의 토끼 (입력을 받아 옴)
        private readonly AugmentCardPanelView view;

        private Mode mode = Mode.Closed;
        private int selected;
        private int lastX, lastY;

        private PlayerType Owner => view.Owner;

        public AugmentCardPresenter(StoreManager store, PlayerMovement player, AugmentCardPanelView view)
        {
            this.store = store;
            this.player = player;
            this.view = view;

            store.AugmentCardsRequested += OnAugmentRequested;
            store.DebuffChoiceRequested += OnDebuffRequested;
            store.Closing += Close;
            store.StateChanged += OnStateChanged;
        }

        public void Dispose()
        {
            store.AugmentCardsRequested -= OnAugmentRequested;
            store.DebuffChoiceRequested -= OnDebuffRequested;
            store.Closing -= Close;
            store.StateChanged -= OnStateChanged;
        }

        private void OnAugmentRequested(PlayerType p) { if (p == Owner) Open(Mode.Augment); }
        private void OnDebuffRequested(PlayerType p) { if (p == Owner) Open(Mode.Debuff); }
        private void OnStateChanged() { if (mode != Mode.Closed) Refresh(); }   // 온라인 판매·새로고침·당근 반영

        private void Open(Mode newMode)
        {
            mode = newMode;
            selected = 0;
            lastX = lastY = 0;

            if (player != null) player.SetInputReceiver(this);
            view.Open();
            if (OnlineSession.IsOnline) view.CenterOnScreen();     // 온라인은 화면이 하나 → 가운데
            Refresh();
        }

        private void Close()
        {
            if (mode == Mode.Closed) return;
            mode = Mode.Closed;
            view.Close();
            if (player != null) player.SetInputReceiver(null);
        }

        // ── 입력 (창이 열려 있는 동안 PlayerMovement가 넘겨줌) ──
        public void OnNavigate(Vector2 input)
        {
            int x = input.x > 0.5f ? 1 : input.x < -0.5f ? -1 : 0;
            int y = input.y > 0.5f ? 1 : input.y < -0.5f ? -1 : 0;

            if (x != 0 && x != lastX)
            {
                selected = Mathf.Clamp(selected + x, 0, view.CardCount - 1);
                view.SetSelected(selected);
            }
            if (y == -1 && lastY != -1 && mode == Mode.Augment) Close();   // 디버프는 꼭 골라야 하므로 닫기 없음

            lastX = x;
            lastY = y;
        }

        public void OnConfirm()
        {
            if (mode == Mode.Augment)
            {
                if (store.TryBuy(Owner, selected)) Close();
                else Refresh();                                     // 당근 부족·구매 완료
            }
            else if (mode == Mode.Debuff)
            {
                store.ChooseDebuff(Owner, Debuffs[selected].type);
                Close();
            }
        }

        // ── 카드 내용 정하기 ──
        private void Refresh()
        {
            if (mode == Mode.Augment)
            {
                view.SetTitle("증강 구매");
                view.SetHint("좌우: 선택   공격: 구매   아래: 닫기");
                StoreOffer[] offers = store.OffersOf(Owner);
                for (int i = 0; i < view.CardCount && i < offers.Length; i++)
                {
                    StoreOffer o = offers[i];
                    if (o.Augment == null) continue;                // 온라인 상대 PC: 카드를 아직 못 받음
                    string price = o.Sold ? "구매 완료" : $"{store.GetPrice(Owner)} 당근";
                    view.ShowCard(i, o.Augment.displayName, o.Augment.Describe(o.Tier),
                        StoreSettings.TagName(o.Augment.tag),           // TODO: 태그 진행도 (예: 이동 2/3)
                        price, store.Settings.tierColors[(int)o.Tier], o.Augment.icon, o.Sold);
                }
            }
            else if (mode == Mode.Debuff)
            {
                view.SetTitle("디버프 선택 (상대에게 다음 라운드 적용)");
                view.SetHint("좌우: 선택   공격: 결정");
                float m = PlayerDataManager.Instance.debuffMultiplier;
                for (int i = 0; i < view.CardCount && i < Debuffs.Length; i++)
                    view.ShowCard(i, Debuffs[i].name, string.Format(Debuffs[i].effect, m), "디버프", "무료", view.DebuffColor, null, false);
            }

            view.SetSelected(selected);
        }
    }
}
