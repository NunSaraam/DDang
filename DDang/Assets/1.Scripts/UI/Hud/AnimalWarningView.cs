using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 난입 동물 경고 (보여 주기만): 아이콘과 이름을 페이드로 띄웠다가 showTime 뒤 닫힘
    public class AnimalWarningView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private float showTime = 3f;
        [SerializeField] private float fadeTime = 0.2f;

        private CanvasGroup group;
        private Tween hideCall;

        private void Awake()
        {
            if (!panel.TryGetComponent(out group)) group = panel.AddComponent<CanvasGroup>();
            panel.SetActive(false);
        }

        public void Show(Sprite sprite, string text)
        {
            icon.gameObject.SetActive(sprite != null);              // 아이콘이 없는 동물은 흰 네모 대신 숨김
            icon.sprite = sprite;
            nameText.text = text;

            group.DOKill();
            panel.transform.DOKill(true);
            panel.SetActive(true);
            group.alpha = 0f;
            group.DOFade(1f, fadeTime);
            panel.transform.localScale = Vector3.one;
            panel.transform.DOPunchScale(Vector3.one * 0.15f, 0.35f, 6, 0.5f);

            hideCall?.Kill();
            hideCall = DOVirtual.DelayedCall(showTime, Hide);
        }

        public void Hide()
        {
            hideCall?.Kill();
            hideCall = null;
            if (!panel.activeSelf) return;
            group.DOKill();
            group.DOFade(0f, fadeTime).OnComplete(() => panel.SetActive(false));
        }

        private void OnDestroy()
        {
            hideCall?.Kill();
            if (group != null) group.DOKill();
            if (panel != null) panel.transform.DOKill();
        }
    }
}
