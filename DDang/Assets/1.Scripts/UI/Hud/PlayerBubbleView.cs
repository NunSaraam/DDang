using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    // 토끼 머리 위 말풍선 (보여 주기만): 게임 중엔 칠한 칸 수 또는 기절 그림, 상점에선 타일 안내 문구
    public class PlayerBubbleView : MonoBehaviour
    {
        public enum Mode { Game, Store }

        [SerializeField] private Image stunImage;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI storeText;
        [SerializeField] private Transform targetCamera;            // 비우면 Main Camera

        private Mode mode = Mode.Game;
        private bool stunned;

        private void LateUpdate()
        {
            if (targetCamera == null && Camera.main != null) targetCamera = Camera.main.transform;
            if (targetCamera != null) transform.LookAt(transform.position + targetCamera.forward);   // 항상 카메라를 봄
        }

        public void SetMode(Mode newMode)
        {
            mode = newMode;
            Apply();
        }

        public void ShowStun(bool on)
        {
            stunned = on;
            Apply();
        }

        public void ShowScore(int value, bool animate)
        {
            scoreText.text = value.ToString();
            if (!animate || !scoreText.gameObject.activeInHierarchy) return;
            scoreText.transform.DOKill(true);
            scoreText.transform.localScale = Vector3.one;
            scoreText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 4, 0.5f);   // 칸 수가 바뀌면 톡
        }

        public void ShowStoreHint(string text)
        {
            storeText.enabled = true;
            storeText.text = text;
        }

        private void Apply()
        {
            stunImage.gameObject.SetActive(mode == Mode.Game && stunned);
            scoreText.gameObject.SetActive(mode == Mode.Game && !stunned);
            storeText.gameObject.SetActive(mode == Mode.Store);
        }

        private void OnDestroy()
        {
            if (scoreText != null) scoreText.transform.DOKill();
        }
    }
}
