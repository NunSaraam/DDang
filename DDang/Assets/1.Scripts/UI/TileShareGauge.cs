using Rules;
using UnityEngine;

namespace UI
{
    // 플레이어별 점유율 게이지: 각자 배경 트랙 안에서 둥근 막대가 늘어나고, 막대 끝을 토끼가 따라감
    public class TileShareGauge : MonoBehaviour
    {
        [System.Serializable]
        private class Gauge
        {
            public RectTransform bar;           // 트랙(배경)의 자식, Image Type: Sliced (둥근 스프라이트)
            public RectTransform runner;        // 트랙의 자식, 막대 끝의 토끼 아이콘
            public bool fromRight;              // P2처럼 오른쪽부터 차오르면 체크

            [HideInInspector] public float shown;       // 화면에 보이는 값 (부드럽게 따라감)
            [HideInInspector] public float velocity;
        }

        [SerializeField] private Gauge p1;
        [SerializeField] private Gauge p2;

        [Header("연출")]
        [SerializeField, Min(0f)] private float fillSmoothTime = 0.25f;  // 목표치까지 차오르는 시간
        [SerializeField] private float hopHeight = 6f;                  // 토끼가 뛰는 높이 (px)
        [SerializeField] private float hopSpeed = 14f;
        [SerializeField] private float movingSpeedThreshold = 0.05f;    // 초당 이 비율 이상 변하면 뛰는 중

        private void Update()
        {
            if (!RoundManager.HasInstance) return;

            RoundManager round = RoundManager.Instance;
            int total = round.PaintableCount;
            if (total == 0) return;

            UpdateGauge(p1, (float)round.GetTiles(PlayerType.Player1) / total);
            UpdateGauge(p2, (float)round.GetTiles(PlayerType.Player2) / total);
        }

        private void UpdateGauge(Gauge g, float target)
        {
            g.shown = Mathf.SmoothDamp(g.shown, target, ref g.velocity, fillSmoothTime);

            // 앵커로 막대 길이 조절 → 트랙 크기와 해상도에 관계없이 비율대로 늘어나고 둥근 끝이 유지됨
            float start = g.fromRight ? 1f - g.shown : 0f;
            float end = g.fromRight ? 1f : g.shown;
            g.bar.anchorMin = new Vector2(start, 0f);
            g.bar.anchorMax = new Vector2(end, 1f);
            g.bar.offsetMin = g.bar.offsetMax = Vector2.zero;
            g.bar.gameObject.SetActive(g.shown > 0.001f);              // 0%일 때 둥근 점만 남지 않게

            if (g.runner == null) return;

            float tip = g.fromRight ? start : end;                       // 막대의 안쪽 끝
            g.runner.anchorMin = g.runner.anchorMax = new Vector2(tip, 0.5f);

            bool moving = Mathf.Abs(g.velocity) > movingSpeedThreshold;
            float hop = moving ? Mathf.Abs(Mathf.Sin(Time.time * hopSpeed)) * hopHeight : 0f;
            g.runner.anchoredPosition = new Vector2(0f, hop);
        }
    }
}
