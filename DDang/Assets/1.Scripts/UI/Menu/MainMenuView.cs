using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 메인 메뉴 화면 (보여 주기만): 버튼 클릭을 이벤트로 알리고, 페이지를 밀어서 바꿈
    public class MainMenuView : MonoBehaviour
    {
        public enum Page { Main, Tutorial }

        [Header("페이지")]
        [SerializeField] private RectTransform mainPage;
        [SerializeField] private RectTransform tutorialPage;
        [SerializeField] private float slideTime = 0.4f;

        [Header("메인 페이지 버튼")]
        [SerializeField] private Button playButton;                 // 게임 시작 → 튜토리얼 페이지
        [SerializeField] private Button onlineButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("튜토리얼 페이지 버튼")]
        [SerializeField] private Button startButton;                // 진짜 시작 (GameScene)
        [SerializeField] private Button backButton;

        public event Action PlayClicked;
        public event Action OnlineClicked;
        public event Action SettingsClicked;
        public event Action QuitClicked;
        public event Action StartClicked;
        public event Action BackClicked;

        public Page Current { get; private set; } = Page.Main;
        public bool IsSliding { get; private set; }

        private Sequence slide;

        private void Awake()
        {
            Bind(playButton, () => PlayClicked?.Invoke());
            Bind(onlineButton, () => OnlineClicked?.Invoke());
            Bind(settingsButton, () => SettingsClicked?.Invoke());
            Bind(quitButton, () => QuitClicked?.Invoke());
            Bind(startButton, () => StartClicked?.Invoke());
            Bind(backButton, () => BackClicked?.Invoke());

            // 처음 배치: 메인 페이지는 가운데, 튜토리얼은 오른쪽 화면 밖
            mainPage.anchoredPosition = Vector2.zero;
            tutorialPage.anchoredPosition = new Vector2(mainPage.rect.width, 0f);
        }

        private static void Bind(Button button, Action onClick)
        {
            if (button != null) button.onClick.AddListener(() => onClick());
        }

        // forward = 다음 페이지가 오른쪽에서 들어옴, false = 뒤로 (왼쪽에서 들어옴)
        public void ShowPage(Page page, bool forward)
        {
            if (page == Current || IsSliding) return;

            RectTransform from = PageOf(Current);
            RectTransform to = PageOf(page);
            float width = from.rect.width * (forward ? 1f : -1f);

            IsSliding = true;
            to.anchoredPosition = new Vector2(width, 0f);
            slide = DOTween.Sequence()
                .Join(from.DOAnchorPosX(-width, slideTime).SetEase(Ease.OutCubic))
                .Join(to.DOAnchorPosX(0f, slideTime).SetEase(Ease.OutCubic))
                .SetUpdate(true)
                .OnComplete(() => IsSliding = false);
            Current = page;
        }

        // 씬 이동 중 연타 방지
        public void SetInteractable(bool on)
        {
            foreach (Button b in new[] { playButton, onlineButton, settingsButton, quitButton, startButton, backButton })
                if (b != null) b.interactable = on;
        }

        private RectTransform PageOf(Page page) => page == Page.Main ? mainPage : tutorialPage;

        private void OnDestroy() => slide?.Kill();
    }
}
