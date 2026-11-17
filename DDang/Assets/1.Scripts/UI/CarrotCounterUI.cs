using Store;
using TMPro;
using UnityEngine;

namespace UI
{
    // 각 플레이어 화면 상단의 당근 개수. 바뀔 때 숫자가 통통 튐
    public class CarrotCounterUI : MonoBehaviour
    {
        [SerializeField] private PlayerType owner;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private RectTransform popTarget;      // 튀는 대상 (비우면 숫자만)
        [SerializeField] private float popScale = 1.25f;
        [SerializeField] private float popTime = 0.2f;

        private CoinManager coins;
        private float popTimer;

        // 씬 첫 로드 때 CoinManager가 먼저 준비되도록 Start에서 연결
        private void Start()
        {
            if (!CoinManager.HasInstance) return;
            coins = CoinManager.Instance;
            coins.CoinsChanged += OnCoinsChanged;
            countText.text = coins.GetCoins(owner).ToString();
        }

        private void OnDestroy()
        {
            if (coins != null) coins.CoinsChanged -= OnCoinsChanged;
        }

        private void OnCoinsChanged(PlayerType player, int value)
        {
            if (player != owner) return;
            countText.text = value.ToString();
            popTimer = popTime;
        }

        private void Update()
        {
            if (popTimer <= 0f) return;
            popTimer -= Time.deltaTime;

            float t = 1f - Mathf.Clamp01(popTimer / popTime);           // 0 → 1
            float scale = 1f + (popScale - 1f) * Mathf.Sin(t * Mathf.PI);
            Target.localScale = Vector3.one * scale;
        }

        private RectTransform Target => popTarget != null ? popTarget : countText.rectTransform;
    }
}
