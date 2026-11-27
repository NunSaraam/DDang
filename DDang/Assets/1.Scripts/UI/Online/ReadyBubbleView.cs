using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    // 대기실 토끼 위 말풍선 (보여 주기만): 닉네임 + Ready / UnReady. 말풍선 Canvas에 붙임
    // 오브젝트를 끄면 스크립트도 멈추므로 Canvas 그리기만 켜고 끔
    public class ReadyBubbleView : MonoBehaviour
    {
        [SerializeField] private PlayerType slot;                       // 어느 토끼의 말풍선인지
        [SerializeField] private TextMeshProUGUI nicknameText;
        [SerializeField] private TextMeshProUGUI stateText;
        [SerializeField] private Color readyColor = new(0.3f, 0.75f, 0.4f);
        [SerializeField] private Color unreadyColor = new(0.6f, 0.6f, 0.6f);

        private Canvas canvas;
        private bool? shownReady;

        public PlayerType Slot => slot;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            nicknameText.gameObject.SetActive(true);                    // 복사해 온 점수 글자는 꺼져 있을 수 있음
            stateText.gameObject.SetActive(true);
            Hide();
        }

        private void LateUpdate()
        {
            if (Camera.main != null) transform.LookAt(transform.position + Camera.main.transform.forward);   // 항상 카메라를 봄
        }

        public void Show(string nickname, bool ready)
        {
            canvas.enabled = true;
            nicknameText.text = nickname;
            stateText.text = ready ? "Ready" : "UnReady";
            stateText.color = ready ? readyColor : unreadyColor;

            if (shownReady.HasValue && shownReady.Value != ready)       // 준비 상태가 바뀌면 톡
            {
                stateText.transform.DOKill(true);
                stateText.transform.localScale = Vector3.one;
                stateText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 6, 0.5f);
            }
            shownReady = ready;
        }

        public void Hide()
        {
            if (canvas != null) canvas.enabled = false;
            shownReady = null;
        }

        private void OnDestroy()
        {
            if (stateText != null) stateText.transform.DOKill();
        }
    }
}
