using DG.Tweening;
using Store;
using TMPro;
using UnityEngine;

namespace UI
{
    // 증강·디버프 카드 창 (보여 주기만). 선택·구매 판단은 AugmentCardPresenter
    public class AugmentCardPanelView : MonoBehaviour
    {
        [SerializeField] private PlayerType owner;                  // 몇 P 창인지
        [SerializeField] private GameObject panel;
        [SerializeField] private AugmentCardView[] cards;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Color debuffColor = new(0.85f, 0.3f, 0.3f);
        [SerializeField] private float openTime = 0.15f;

        private CanvasGroup group;

        public PlayerType Owner => owner;
        public int CardCount => cards.Length;
        public Color DebuffColor => debuffColor;

        private void Awake()
        {
            if (!panel.TryGetComponent(out group)) group = panel.AddComponent<CanvasGroup>();
            panel.SetActive(false);
        }

        public void Open()
        {
            group.DOKill();
            panel.transform.DOKill();
            panel.SetActive(true);
            group.alpha = 0f;
            group.DOFade(1f, openTime).SetUpdate(true);
            panel.transform.localScale = Vector3.one * 0.9f;
            panel.transform.DOScale(1f, openTime).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void Close()
        {
            if (!panel.activeSelf) return;
            group.DOKill();
            group.DOFade(0f, openTime).SetUpdate(true).OnComplete(() => panel.SetActive(false));
        }

        public void SetTitle(string text) => titleText.text = text;
        public void SetHint(string text) => hintText.text = text;

        public void ShowCard(int index, string title, string effect, string tag, string price, Color color, Sprite icon, bool sold) =>
            cards[index].Show(title, effect, tag, price, color, icon, sold);

        public void SetSelected(int index)
        {
            for (int i = 0; i < cards.Length; i++) cards[i].SetSelected(i == index);
        }

        // 화면 반쪽(1P 왼쪽 / 2P 오른쪽)에 있던 카드 창을 가로 가운데로 (온라인은 화면이 하나)
        public void CenterOnScreen()
        {
            var rect = (RectTransform)panel.transform;
            Transform canvas = panel.GetComponentInParent<Canvas>().rootCanvas.transform;
            rect.position = new Vector3(canvas.position.x, rect.position.y, rect.position.z);
        }

        private void OnDestroy()
        {
            if (group != null) group.DOKill();
            if (panel != null) panel.transform.DOKill();
        }
    }
}
