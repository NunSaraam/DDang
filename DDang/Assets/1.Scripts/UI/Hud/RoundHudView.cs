using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 게임 화면의 라운드 표시: 시작 전 카운트다운, 남은 시간, 라운드 승리 아이콘 (보여 주기만, 판단은 Presenter)
    public class RoundHudView : MonoBehaviour
    {
        [Header("시작 전 카운트다운")]
        [SerializeField] private UIPanel waitPanel;                 // StartWaitPanel에 UIPanel(+CanvasGroup)
        [SerializeField] private TextMeshProUGUI waitText;

        [Header("남은 시간")]
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("라운드 승리 아이콘")]
        [SerializeField] private Image[] p1WinImages;
        [SerializeField] private Image[] p2WinImages;
        [SerializeField] private Sprite emptySprite;
        [SerializeField] private Sprite p1WinSprite;
        [SerializeField] private Sprite p2WinSprite;

        [Header("연출")]
        [SerializeField] private float countPunch = 0.25f;          // 카운트다운 숫자가 1초마다 튀는 크기
        [SerializeField] private float winPunch = 0.4f;             // 새 승리 아이콘이 튀는 크기

        private int lastWaitSecond = -1;
        private int shownP1 = -1, shownP2 = -1;                     // -1 = 처음 (처음에는 튀지 않음)

        public void ShowWait(bool show)
        {
            if (show) waitPanel.Show();
            else waitPanel.Hide();
        }

        public void SetWaitTime(float seconds)
        {
            waitText.text = $"{seconds:F1}";
            int second = Mathf.CeilToInt(seconds);
            if (second == lastWaitSecond) return;
            lastWaitSecond = second;
            Punch(waitText.transform, countPunch);                  // 3 → 2 → 1 바뀔 때마다 통
        }

        public void SetRoundTime(float seconds) => timeText.text = $"{seconds:F2}";

        public void ClearTimes()
        {
            waitText.text = "";
            timeText.text = "";
            lastWaitSecond = -1;
        }

        public void SetWins(int p1Wins, int p2Wins)
        {
            Fill(p1WinImages, p1Wins, shownP1, p1WinSprite);
            Fill(p2WinImages, p2Wins, shownP2, p2WinSprite);
            shownP1 = p1Wins;
            shownP2 = p2Wins;
        }

        // 새로 채워진 칸만 톡 (처음 표시하거나 줄어들 때는 그냥 바꿈)
        private void Fill(Image[] images, int wins, int shown, Sprite winSprite)
        {
            for (int i = 0; i < images.Length; i++)
            {
                images[i].sprite = i < wins ? winSprite : emptySprite;
                if (shown >= 0 && i >= shown && i < wins) Punch(images[i].transform, winPunch);
            }
        }

        private static void Punch(Transform target, float strength)
        {
            target.DOKill(true);
            target.localScale = Vector3.one;
            target.DOPunchScale(Vector3.one * strength, 0.3f, 6, 0.5f).SetUpdate(true);
        }

        private void OnDestroy()
        {
            if (waitText != null) waitText.transform.DOKill();
            foreach (Image i in p1WinImages) if (i != null) i.transform.DOKill();
            foreach (Image i in p2WinImages) if (i != null) i.transform.DOKill();
        }
    }
}
