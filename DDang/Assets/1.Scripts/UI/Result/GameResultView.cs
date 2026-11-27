using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 최종 결과 화면 (보여 주기만): 두 패널이 깜빡이다가 승자는 진하게, 패자는 흐리게 + 승/패 마크
    public class GameResultView : MonoBehaviour
    {
        [SerializeField] private Image player1Panel;
        [SerializeField] private GameObject p1Win;
        [SerializeField] private GameObject p1Lose;

        [SerializeField] private Image player2Panel;
        [SerializeField] private GameObject p2Win;
        [SerializeField] private GameObject p2Lose;

        [Header("연출")]
        [SerializeField] private float pulseDuration = 3f;         // 깜빡이는 시간
        [SerializeField] private float pulseInterval = 0.5f;       // 한 번 흐려졌다 돌아오는 절반 시간

        private Sequence sequence;

        private void Awake()
        {
            foreach (GameObject mark in new[] { p1Win, p1Lose, p2Win, p2Lose }) mark.SetActive(false);
        }

        public void PlayResult(PlayerType winner)
        {
            int loops = Mathf.Max(2, Mathf.RoundToInt(pulseDuration / pulseInterval));
            if (loops % 2 == 1) loops++;                            // 짝수 번 = 원래 밝기로 끝남

            sequence = DOTween.Sequence()
                .Join(player1Panel.DOFade(0.5f, pulseInterval).SetLoops(loops, LoopType.Yoyo))
                .Join(player2Panel.DOFade(0.5f, pulseInterval).SetLoops(loops, LoopType.Yoyo))
                .AppendCallback(() => ShowWinner(winner));
        }

        private void ShowWinner(PlayerType winner)
        {
            bool p1Won = winner == PlayerType.Player1;
            bool p2Won = winner == PlayerType.Player2;
            if (!p1Won && !p2Won) return;

            player1Panel.DOFade(p1Won ? 1f : 0.5f, 0.2f);
            player2Panel.DOFade(p2Won ? 1f : 0.5f, 0.2f);
            Pop(p1Won ? p1Win : p1Lose);
            Pop(p2Won ? p2Win : p2Lose);
        }

        private static void Pop(GameObject mark)
        {
            mark.SetActive(true);
            mark.transform.localScale = Vector3.zero;
            mark.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
        }

        private void OnDestroy()
        {
            sequence?.Kill();
            if (player1Panel != null) player1Panel.DOKill();
            if (player2Panel != null) player2Panel.DOKill();
            foreach (GameObject mark in new[] { p1Win, p1Lose, p2Win, p2Lose })
                if (mark != null) mark.transform.DOKill();
        }
    }
}
