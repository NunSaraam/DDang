using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    // 듀스 / 결투 / 추가 시간 / 서든데스 / 골든골 문구 (보여 주기만, 문구는 RoundStatusPresenter가 정함)
    public class RoundStatusView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statusText;

        public void SetText(string text)
        {
            bool show = !string.IsNullOrEmpty(text);
            bool changed = show && text != statusText.text;
            statusText.gameObject.SetActive(show);
            statusText.text = text;
            if (changed) Pop(statusText.transform);                 // 새 문구가 나오면 톡
        }

        private static void Pop(Transform t)
        {
            t.DOKill(true);
            t.localScale = Vector3.one;
            t.DOPunchScale(Vector3.one * 0.2f, 0.3f, 6, 0.5f).SetUpdate(true);
        }

        private void OnDestroy()
        {
            if (statusText != null) statusText.transform.DOKill();
        }
    }
}
