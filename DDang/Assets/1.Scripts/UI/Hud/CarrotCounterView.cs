using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    // 각 플레이어 화면 상단의 당근 개수 (보여 주기만). 바뀔 때 숫자가 통통 튐
    public class CarrotCounterView : MonoBehaviour
    {
        [SerializeField] private PlayerType owner;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private RectTransform popTarget;      // 튀는 대상 (비우면 숫자만)
        [SerializeField] private float popScale = 1.25f;
        [SerializeField] private float popTime = 0.2f;

        public PlayerType Owner => owner;

        private Transform Target => popTarget != null ? popTarget : countText.rectTransform;

        public void SetCount(int value, bool animate)
        {
            countText.text = value.ToString();
            if (!animate) return;

            Transform t = Target;
            t.DOKill(true);
            t.localScale = Vector3.one;
            t.DOPunchScale(Vector3.one * (popScale - 1f), popTime, 4, 0.5f).SetUpdate(true);
        }

        private void OnDestroy()
        {
            if (countText != null) Target.DOKill();
        }
    }
}
