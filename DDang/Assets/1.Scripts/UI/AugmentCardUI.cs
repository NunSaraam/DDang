using Stats;
using TMPro;
using UnityEngine;

namespace Store
{
    // 열려 있는 동안 이동 잠금, 좌우 = 선택, 공격 = 구매, 아래 = 닫기
    public class AugmentCardUI : MonoBehaviour, IPlayerInputReceiver
    {
        private enum Mode { Closed, Augment, Debuff }

        [SerializeField] private PlayerType owner;
        [SerializeField] private GameObject panel;
        [SerializeField] private AugmentCardView[] cards;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Color debuffColor = new(0.85f, 0.3f, 0.3f);

        private static readonly (DebuffType type, string name, string effect)[] Debuffs =
        {
            (DebuffType.MoveSpeedDown, "이속 감소", "상대 이동속도 ÷{0}"),
            (DebuffType.StunTimeUp, "기절 시간 증가", "상대 기절 시간 ×{0}"),
            (DebuffType.AttackCooldownUp, "공격 쿨타임 증가", "상대 공격 쿨타임 ×{0}"),
        };

        private Mode mode = Mode.Closed;
        private StoreOffer[] offers;
        private int selected;
        private int lastX, lastY;
        private PlayerMovement player;

        private void Awake() => panel.SetActive(false);

        public void OpenAugments(StoreOffer[] newOffers)
        {
            offers = newOffers;
            Open(Mode.Augment);
        }

        public void OpenDebuffs() => Open(Mode.Debuff);

        public void Close()
        {
            if (mode == Mode.Closed) return;
            mode = Mode.Closed;
            panel.SetActive(false);
            if (player != null) player.SetInputReceiver(null);
        }

        private void Open(Mode newMode)
        {
            mode = newMode;
            selected = 0;
            lastX = lastY = 0;

            if (player == null) player = FindPlayer();
            if (player != null) player.SetInputReceiver(this);

            panel.SetActive(true);
            Refresh();
        }

        public void OnNavigate(Vector2 input)
        {
            int x = input.x > 0.5f ? 1 : input.x < -0.5f ? -1 : 0;
            int y = input.y > 0.5f ? 1 : input.y < -0.5f ? -1 : 0;

            if (x != 0 && x != lastX)
            {
                selected = Mathf.Clamp(selected + x, 0, cards.Length - 1);
                Refresh();
            }
            if (y == -1 && lastY != -1 && mode == Mode.Augment) Close();   // 디버프는 꼭 골라야 하므로 닫기 없음

            lastX = x;
            lastY = y;
        }

        public void OnConfirm()
        {
            if (mode == Mode.Augment)
            {
                if (StoreManager.Current.TryBuy(owner, selected)) Close();
                else Refresh();                                     // 당근 부족·구매 완료
            }
            else if (mode == Mode.Debuff)
            {
                StoreManager.Current.ChooseDebuff(owner, Debuffs[selected].type);
                Close();
            }
        }

        private void Refresh()
        {
            StoreManager store = StoreManager.Current;

            if (mode == Mode.Augment)
            {
                titleText.text = "증강 구매";
                hintText.text = "좌우: 선택   공격: 구매   아래: 닫기";
                for (int i = 0; i < cards.Length; i++)
                {
                    StoreOffer o = offers[i];
                    string price = o.Sold ? "구매 완료" : $"{store.GetPrice(owner)} 당근";
                    cards[i].Show(o.Augment.displayName, o.Augment.Describe(o.Tier),
                        StoreSettings.TagName(o.Augment.tag),           // TODO: 태그 진행도 (예: 이동 2/3)
                        price, store.Settings.tierColors[(int)o.Tier], o.Augment.icon, o.Sold);
                }
            }
            else
            {
                titleText.text = "디버프 선택 (상대에게 다음 라운드 적용)";
                hintText.text = "좌우: 선택   공격: 결정";
                float m = PlayerDataManager.Instance.debuffMultiplier;
                for (int i = 0; i < cards.Length; i++)
                    cards[i].Show(Debuffs[i].name, string.Format(Debuffs[i].effect, m), "디버프", "무료", debuffColor, null, false);
            }

            for (int i = 0; i < cards.Length; i++) cards[i].SetSelected(i == selected);
        }

        private PlayerMovement FindPlayer()
        {
            foreach (PlayerMovement p in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
                if (p.playerType == owner) return p;
            return null;
        }
    }
}
