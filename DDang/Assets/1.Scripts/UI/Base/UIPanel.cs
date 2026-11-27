using System;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    // 모든 창(패널)의 공통 기반: 페이드로 열고 닫기. 시간이 멈춰도(일시정지) 동작
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        [SerializeField] private float fadeDuration = 0.15f;

        private CanvasGroup canvasGroup;
        private CanvasGroup Group => canvasGroup != null ? canvasGroup : canvasGroup = GetComponent<CanvasGroup>();

        public bool IsOpen { get; private set; }                    // 닫히는 중에도 activeSelf는 true라서 따로 기록

        public event Action CloseRequested;                         // 창 안의 "닫기" 버튼 → 닫는 방법은 창을 연 쪽이 정함
        public void RequestClose() => CloseRequested?.Invoke();

        public virtual void Show()
        {
            IsOpen = true;
            if (!gameObject.activeSelf) Group.alpha = 0f;
            gameObject.SetActive(true);

            Group.DOKill();
            Group.interactable = true;
            Group.blocksRaycasts = true;
            Group.DOFade(1f, fadeDuration).SetUpdate(true);         // SetUpdate(true) = 실제 시간 기준
        }

        public virtual void Hide()
        {
            IsOpen = false;
            Group.DOKill();
            Group.interactable = false;
            Group.blocksRaycasts = false;
            Group.DOFade(0f, fadeDuration).SetUpdate(true)
                 .OnComplete(() => gameObject.SetActive(false));
        }

        public void HideImmediate()
        {
            IsOpen = false;
            Group.DOKill();
            Group.alpha = 0f;
            Group.interactable = false;
            Group.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        // 보이는 채로 입력만 막거나 풀기
        public void SetInteractable(bool interactable)
        {
            Group.interactable = interactable;
            Group.blocksRaycasts = interactable;
        }

        protected virtual void OnDestroy() => Group.DOKill();
    }
}
