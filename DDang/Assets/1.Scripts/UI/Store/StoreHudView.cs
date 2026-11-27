using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    // 상점 남은 시간 (보여 주기만). 5초 이하에서는 1초마다 톡
    public class StoreHudView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI shopingTimeText;
        [SerializeField] private float urgentTime = 5f;

        private int lastSecond = -1;

        public void SetTime(float seconds)
        {
            shopingTimeText.text = $"{seconds:F2}";
            int second = Mathf.CeilToInt(seconds);
            if (seconds > urgentTime || second == lastSecond) return;
            lastSecond = second;

            Transform t = shopingTimeText.transform;
            t.DOKill(true);
            t.localScale = Vector3.one;
            t.DOPunchScale(Vector3.one * 0.2f, 0.25f, 6, 0.5f).SetUpdate(true);
        }

        private void OnDestroy()
        {
            if (shopingTimeText != null) shopingTimeText.transform.DOKill();
        }
    }
}
